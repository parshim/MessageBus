using System;
using System.Collections.Generic;

using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RabbitMQBus : IBus
    {
        private readonly IConnection _connection;

        private readonly IMessageHelper _messageHelper = new MessageHelper();
        private readonly ISendHelper _sendHelper = new SendHelper();

        private readonly Func<Action<ISubscriberConfigurator>, SubscriberConfigurator> _createSubscriberConfigurator;
        private readonly Func<Action<IPublisherConfigurator>, PublisherConfigurator> _createPublisherConfigurator;
        private readonly Func<Action<IRpcPublisherConfigurator>, RpcPublisherConfigurator> _createRpcPublisherConfigurator;

        private readonly string _exchange;

        public RabbitMQBus(Action<IBusConfigurator> busConfigurator = null)
        {
            BusConfigurator busConfiguration = new BusConfigurator();

            if (busConfigurator != null)
            {
                busConfigurator(busConfiguration);
            }

            BusId = busConfiguration.BusId;

            _exchange = "amq.headers";

            string username = "guest";
            string password = "guest";

            if (!string.IsNullOrEmpty(busConfiguration.ConnectionString.Endpoint))
            {
                _exchange = busConfiguration.ConnectionString.Endpoint;
            }

            if (!string.IsNullOrEmpty(busConfiguration.ConnectionString.Username))
            {
                username = busConfiguration.ConnectionString.Username;
            }

            if (!string.IsNullOrEmpty(busConfiguration.ConnectionString.Password))
            {
                password = busConfiguration.ConnectionString.Password;
            }
            
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = busConfiguration.ConnectionString.Host,
                Port = busConfiguration.ConnectionString.Port,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                UserName = username,
                Password = password,
                VirtualHost = busConfiguration.ConnectionString.VirtualHost
            };

            _connection = factory.CreateConnection();

            _createSubscriberConfigurator = configure =>
            {
                SubscriberConfigurator configurator = new SubscriberConfigurator(_exchange, busConfiguration.ReplyExchange, busConfiguration.ErrorSubscriber, busConfiguration.ReceiveSelfPublish, busConfiguration.Trace);

                if (configure != null)
                {
                    configure(configurator);
                }

                return configurator;
            };

            _createPublisherConfigurator = configure =>
            {
                PublisherConfigurator configurator = new PublisherConfigurator(_exchange, busConfiguration.ErrorHandler, busConfiguration.Trace);

                if (configure != null)
                {
                    configure(configurator);
                }

                return configurator;
            };

            _createRpcPublisherConfigurator = configure =>
            {
                RpcPublisherConfigurator configurator = new RpcPublisherConfigurator(_exchange, busConfiguration.UseFastReply, busConfiguration.ReplyExchange, busConfiguration.ErrorHandler, busConfiguration.Trace);

                if (configure != null)
                {
                    configure(configurator);
                }

                return configurator;
            };
        }

        public string BusId { get; private set; }
        
        public void Dispose()
        {
            _connection.Dispose();
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

        public ISubscription CreateMonitor(Action<RawBusMessage> monitor)
        {
            SubscriberConfigurator configurator = _createSubscriberConfigurator(null);
            
            IModel model = _connection.CreateModel();

            string queue = CreateQueue(model, configurator);

            model.QueueBind(queue, configurator.Exchange, configurator.RoutingKey);

            var consumer = new MessageMonitorConsumer(_messageHelper, monitor);

            return new MessageMonitor(model, queue, consumer, configurator);
        }

        private static string CreateQueue(IModel model, SubscriberConfigurator configurator)
        {
            QueueDeclareOk queueDeclare;

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
                        model.QueueDeclare(configurator.QueueName, false, true, true, arguments);
                    }
                }

                return configurator.QueueName;
            }

            queueDeclare = model.QueueDeclare("", false, true, true, arguments);

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
