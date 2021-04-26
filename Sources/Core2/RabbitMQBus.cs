using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MessageBus.Core.API;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace MessageBus.Core
{
    public class RabbitMQBus : IBus
    {
        private IConnection _connection;

        private readonly IMessageHelper _messageHelper = new MessageHelper();
        private readonly ISendHelper _sendHelper = new SendHelper();

        private readonly Func<Action<ISubscriberConfigurator>, SubscriberConfigurator> _createSubscriberConfigurator;
        private readonly Func<Action<IPublisherConfigurator>, PublisherConfigurator> _createPublisherConfigurator;
        private readonly Func<Action<IRpcPublisherConfigurator>, RpcPublisherConfigurator> _createRpcPublisherConfigurator;

        private readonly ManualResetEvent _recovery = new ManualResetEvent(true);

        private string _exchange;

        public RabbitMQBus(Action<IBusConfigurator> busConfigurator = null)
        {
            BusConfigurator busConfiguration = new BusConfigurator();

            busConfigurator?.Invoke(busConfiguration);

            BusId = busConfiguration.BusId;
            BusConnectionName = busConfiguration.ConnectionProvidedName;

            _exchange = "amq.headers";
            
            var factory = ConnectionFactory(busConfiguration.ConnectionString);

            var ex = Connect(busConfiguration, factory);

            if (ex != null)
            {
                factory = ConnectionFactory(busConfiguration.AlternateConnectionString);

                ex = Connect(busConfiguration, factory);
            }

            if (ex != null)
                throw ex;
            
            _connection.ConnectionBlocked += (sender, args) =>
            {
                busConfiguration.ConnectionBlocked(args.Reason);
            };

            _connection.ConnectionUnblocked += (sender, args) =>
            {
                busConfiguration.ConnectionUnblocked();
            };

            _connection.ConnectionShutdown += (sender, args) =>
            {
                _recovery.Reset();
            };

            _connection.RecoverySucceeded += (sender, args) =>
            {
                _recovery.Set();
            };

            _createSubscriberConfigurator = configure =>
            {
                SubscriberConfigurator configurator = new SubscriberConfigurator(_exchange, busConfiguration.ReplyExchange, busConfiguration.ErrorSubscriber, busConfiguration.ReceiveSelfPublish, busConfiguration.Trace, () => busConfiguration.Blocked);

                configure?.Invoke(configurator);

                return configurator;
            };

            _createPublisherConfigurator = configure =>
            {
                PublisherConfigurator configurator = new PublisherConfigurator(_exchange, busConfiguration.ErrorHandler, busConfiguration.Trace, () => busConfiguration.Blocked);

                configure?.Invoke(configurator);

                return configurator;
            };

            _createRpcPublisherConfigurator = configure =>
            {
                RpcPublisherConfigurator configurator = new RpcPublisherConfigurator(_exchange, busConfiguration.UseFastReply, busConfiguration.ReplyExchange, busConfiguration.ErrorHandler, busConfiguration.Trace, () => busConfiguration.Blocked);

                configure?.Invoke(configurator);

                return configurator;
            };
        }

        private Exception Connect(BusConfigurator busConfiguration, ConnectionFactory factory)
        {
            Exception ex = null;

            for (int i = 0; i < busConfiguration.ConnectionRetries; i++)
            {
                try
                {
                    _connection = factory.CreateConnection(busConfiguration.ConnectionProvidedName);

                    ex = null;

                    break;
                }
                catch (BrokerUnreachableException e)
                {
                    ex = e;
                }
            }

            return ex;
        }

        private ConnectionFactory ConnectionFactory(RabbitMQConnectionString connectionString)
        {
            string username = "guest";
            string password = "guest";

            if (!string.IsNullOrEmpty(connectionString.Endpoint))
            {
                _exchange = connectionString.Endpoint;
            }

            if (!string.IsNullOrEmpty(connectionString.Username))
            {
                username = connectionString.Username;
            }

            if (!string.IsNullOrEmpty(connectionString.Password))
            {
                password = connectionString.Password;
            }

            var factory = new ConnectionFactory
            {
                HostName = connectionString.Host,
                Port = connectionString.Port,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                UserName = username,
                Password = password,
                VirtualHost = connectionString.VirtualHost,
                RequestedConnectionTimeout = 60000
            };

            return factory;
        }

        public string BusId { get; }

        public string BusConnectionName { get; }
        
        public bool IsConnected => _connection?.IsOpen == true;

        public void Dispose()
        {
            _connection.Close(0);
        }
        
        public IPublisher CreatePublisher(Action<IPublisherConfigurator> configure = null)
        {
            PublisherConfigurator configuration = _createPublisherConfigurator(configure);

            IModel model = _connection.CreateModel();

            return new Publisher(model, BusId, configuration, _messageHelper, _sendHelper);
        }

        public ITransactionalPublisher CreateTransactionalPublisher(Action<IPublisherConfigurator> configure = null)
        {
            PublisherConfigurator configuration = _createPublisherConfigurator(configure);

            IModel model = _connection.CreateModel();

            return new TransactionalPublisher(model, BusId, configuration, _messageHelper, _sendHelper);
        }

        public IConfirmPublisher CreateConfirmPublisher(Action<IPublisherConfigurator> configure = null)
        {
            PublisherConfigurator configuration = _createPublisherConfigurator(configure);

            IModel model = _connection.CreateModel();

            return new ConfirmPublisher(model, BusId, configuration, _messageHelper, _sendHelper);
        }

        public IRpcPublisher CreateRpcPublisher(Action<IRpcPublisherConfigurator> configure = null)
        {
            RpcPublisherConfigurator configuration = _createRpcPublisherConfigurator(configure);

            IModel model = _connection.CreateModel();

            IRpcConsumer consumer = new RpcConsumer(BusId, model, _messageHelper, new Dictionary<string, ISerializer>
            {
                { configuration.Serializer.ContentType, configuration.Serializer }
            }, configuration.Trace);

            return new RpcSyncPublisher(model, BusId, configuration, _messageHelper, _sendHelper, consumer);
        }

        public IRpcAsyncPublisher CreateAsyncRpcPublisher(Action<IRpcPublisherConfigurator> configure = null)
        {
            RpcPublisherConfigurator configuration = _createRpcPublisherConfigurator(configure);

            IModel model = _connection.CreateModel();

            IRpcConsumer consumer = new RpcConsumer(BusId, model, _messageHelper, new Dictionary<string, ISerializer>
            {
                { configuration.Serializer.ContentType, configuration.Serializer }
            }, configuration.Trace);

            return new RpcAsyncPublisher(model, BusId, configuration, _messageHelper, _sendHelper, consumer);
        }

        public IReceiver CreateReceiver(Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = _createSubscriberConfigurator(configure);

            IModel model = _connection.CreateModel();

            string queue = CreateQueue(model, configurator);

            return new Receiver(model, BusId, queue, _messageHelper, configurator);
        }

        public ISubscriber CreateSubscriber(Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = _createSubscriberConfigurator(configure);

            IModel model = _connection.CreateModel();

            string queue = CreateQueue(model, configurator);

            IMessageConsumer consumer = CreateConsumer(model, configurator);

            var helper = CreateSubscriptionHelper(consumer, model, queue, configurator);

            return new Subscriber(model, queue, consumer, helper, configurator);
        }

        public ISubscription CreateMonitor(Action<SerializedBusMessage> monitor, Action<ISubscriberConfigurator> configure = null, IEnumerable<BusHeader> filterHeaders = null)
        {
            SubscriberConfigurator configurator = _createSubscriberConfigurator(configure);

            IModel model = _connection.CreateModel();

            string queue = CreateQueue(model, configurator);

            model.QueueBind(queue, configurator.Exchange, configurator.RoutingKey, filterHeaders ?? Enumerable.Empty<BusHeader>());

            DefaultBasicConsumer consumer;

            if (configurator.TransactionalDelivery)
            {
                consumer = new TransactionalMessageMonitorConsumer(model, _messageHelper, monitor, configurator.ExceptionFilter);
            }
            else
            {
                consumer = new MessageMonitorConsumer(model, _messageHelper, monitor, configurator.ErrorSubscriber);
            }

            return new MessageMonitor(model, queue, consumer, configurator);
        }

        public bool WaitRecovery(TimeSpan timeOut)
        {
            return _recovery.WaitOne(timeOut);
        }

        public IAsyncSubscriber CreateAsyncSubscriber(Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = _createSubscriberConfigurator(configure);

            IModel model = _connection.CreateModel();

            string queue = CreateQueue(model, configurator);

            IMessageConsumer consumer = CreateConsumer(model, configurator);

            var helper = CreateSubscriptionHelper(consumer, model, queue, configurator);

            return new AsyncSubscriber(model, queue, consumer, helper, configurator);
        }
        
        public ISubscription RegisterSubscription<T>(T instance, Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = _createSubscriberConfigurator(configure);

            IModel model = _connection.CreateModel();

            string queue = CreateQueue(model, configurator);

            IMessageConsumer consumer = CreateConsumer(model, configurator);

            ISubscriptionHelper helper = CreateSubscriptionHelper(consumer, model, queue, configurator);

            return new Subscription(model, queue, consumer, instance, configurator, helper);
        }

        public IRouteManager CreateRouteManager()
        {
            IModel model = _connection.CreateModel();

            return new RouteManager(model, _exchange);
        }

        private static string CreateQueue(IModel model, SubscriberConfigurator configurator)
        {
            var arguments = new Dictionary<string, object>();

            if (configurator.MaxPriority > 0)
            {
                arguments.Add("x-max-priority", configurator.MaxPriority);
            }

            if (!string.IsNullOrEmpty(configurator.QueueName))
            {
                if (configurator.AutoCreate)
                {
                    if (configurator.Durable)
                    {
                        // exclusive: Can only be accessed by the current connection. (default false)
                        // https://github.com/EasyNetQ/EasyNetQ/wiki/The-Advanced-API
                        model.QueueDeclare(configurator.QueueName, true, false, false, arguments);
                    }
                    else
                    {
                        model.QueueDeclare(configurator.QueueName, false, configurator.IsExclusive, true, arguments);
                    }
                }

                return configurator.QueueName;
            }

            var queueDeclare = model.QueueDeclare("", false, true, true, arguments);

            return queueDeclare.QueueName;
        }

        private static ISubscriptionHelper CreateSubscriptionHelper(IMessageConsumer consumer, IModel model, string queue, SubscriberConfigurator configurator)
        {
            ISubscriptionHelper helper = new SubscriptionHelper((type, filterInfo, handler) =>
            {
                if (consumer.Register(type, filterInfo, handler) && configurator.CreateBindings)
                {
                    model.QueueBind(queue, configurator.Exchange, configurator.RoutingKey, filterInfo);

                    return true;
                }

                return false;
            });
            return helper;
        }

        private MessageConsumer CreateConsumer(IModel model, SubscriberConfigurator configurator)
        {
            if (configurator.TransactionalDelivery)
            {
                return new TransactionalMessageConsumer(BusId, model, _messageHelper, _sendHelper, configurator.ExceptionFilter, configurator.Serializers, configurator.ErrorSubscriber, configurator.TaskScheduler, configurator.ReceiveSelfPublish, configurator.NeverReply, configurator.ReplyExchange, configurator.Trace);
            }

            return new MessageConsumer(BusId, model, _messageHelper, _sendHelper, configurator.Serializers, configurator.ErrorSubscriber, configurator.TaskScheduler, configurator.ReceiveSelfPublish, configurator.NeverReply, configurator.ReplyExchange, configurator.Trace);
        }
    }
}
