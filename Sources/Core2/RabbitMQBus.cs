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

        private readonly Func<Action<ISubscriberConfigurator>, SubscriberConfigurator> _createSubscriberConfigurator;
        private readonly Func<Action<IPublisherConfigurator>, PublisherConfigurator> _createPublisherConfigurator;

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
                SubscriberConfigurator configurator = new SubscriberConfigurator(_exchange, busConfiguration.ErrorSubscriber, busConfiguration.ReceiveSelfPublish);

                if (configure != null)
                {
                    configure(configurator);
                }

                return configurator;
            };

            _createPublisherConfigurator = configure =>
            {
                PublisherConfigurator configurator = new PublisherConfigurator(_exchange, busConfiguration.ErrorHandler);

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

            return new Publisher(model, BusId, configuration, _messageHelper);
        }

        public ITransactionalPublisher CreateTransactionalPublisher(Action<IPublisherConfigurator> configure = null)
        {
            PublisherConfigurator configuration = _createPublisherConfigurator(configure);

            IModel model = _connection.CreateModel();

            return new TransactionalPublisher(model, BusId, configuration, _messageHelper);
        }

        public IConfirmPublisher CreateConfirmPublisher(Action<IPublisherConfigurator> configure = null)
        {
            PublisherConfigurator configuration = _createPublisherConfigurator(configure);

            IModel model = _connection.CreateModel();

            return new ConfirmPublisher(model, BusId, configuration, _messageHelper);
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

            return new Subscriber(model, queue, consumer, configurator);
        }
        
        public ISubscription RegisterSubscription<T>(T instance, Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = _createSubscriberConfigurator(configure);

            IModel model = _connection.CreateModel();

            string queue = CreateQueue(model, configurator);

            IMessageConsumer consumer = CreateConsumer(model, configurator);

            return new Subscription(model, queue, consumer, instance, configurator);
        }

        public IRouteManager CreateRouteManager()
        {
            IModel model = _connection.CreateModel();

            return new RouteManager(model, _exchange);
        }

        private static string CreateQueue(IModel model, SubscriberConfigurator configurator)
        {
            if (!string.IsNullOrEmpty(configurator.QueueName))
            {
                return configurator.QueueName;
            }

            QueueDeclareOk queueDeclare = model.QueueDeclare("", false, true, true, new Dictionary<string, object>());

            return queueDeclare.QueueName;
        }

        private MessageConsumer CreateConsumer(IModel model, SubscriberConfigurator configurator)
        {
            if (configurator.TransactionalDelivery)
            {
                return new TransactionalMessageConsumer(model, BusId, _messageHelper, configurator.Serializers, configurator.ErrorSubscriber, configurator.TaskScheduler, configurator.ReceiveSelfPublish);
            }

            return new MessageConsumer(model, BusId, _messageHelper, configurator.Serializers, configurator.ErrorSubscriber, configurator.TaskScheduler, configurator.ReceiveSelfPublish);
        }
    }
}
