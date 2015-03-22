using System;
using System.Collections.Generic;
using System.Configuration;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RabbitMQBus : IBus
    {
        private readonly IConnection _connection;

        private readonly IMessageHelper _messageHelper = new MessageHelper();
        private readonly ISerializerHelper _serializerHelper = new SerializerHelper();

        private readonly string _exchange;

        public RabbitMQBus() : this(Guid.NewGuid().ToString()) { }
        
        public RabbitMQBus(string busId) : this(busId, "localhost", 5672, "amq.headers") { }

        public RabbitMQBus(string busId, string host, int port, string exchange)
        {
            BusId = busId;

            RabbitMQBusConfigSectionHandler section = ConfigurationManager.GetSection(RabbitMQBusConfigSectionHandler.SectionName) as RabbitMQBusConfigSectionHandler;

            host = GetPropertyValue(host, "localhost", section, s => s.BrokerHost);
            port = GetPropertyValue(port, 5672, section, s => s.Port);

            _exchange = GetPropertyValue(exchange, "amq.headers", section, s => s.Exchange);

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            _connection = factory.CreateConnection();
        }

        public string BusId { get; private set; }

        protected T GetPropertyValue<T>(T value, T defaultValue, RabbitMQBusConfigSectionHandler section, Func<RabbitMQBusConfigSectionHandler, T> selector)
        {
            if (!Equals(value, default(T))) return value;

            if (section == null) return defaultValue;

            return selector(section);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
        
        public IPublisher CreatePublisher(Action<IPublisherConfigurator> configure = null)
        {
            PublisherConfigurator configuration = new PublisherConfigurator();

            if (configure != null)
            {
                configure(configuration);
            }

            IModel model = _connection.CreateModel();

            return new Publisher(model, BusId, _exchange, configuration, _messageHelper, _serializerHelper);
        }

        public IReceiver CreateReceiver(Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = CreateConfigurator(configure);

            IModel model = _connection.CreateModel();

            QueueDeclareOk queue = CreateQueue(model, configurator);

            return new Receiver(model, BusId, _exchange, queue.QueueName, _messageHelper, _serializerHelper, configurator.ErrorSubscriber, configurator.ReceiveSelfPublish);
        }

        public ISubscriber CreateSubscriber(Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = CreateConfigurator(configure);

            IModel model = _connection.CreateModel();

            QueueDeclareOk queue = CreateQueue(model, configurator);

            IMessageConsumer consumer = new MessageConsumer(model, BusId, _messageHelper, _serializerHelper, configurator.ErrorSubscriber, configurator.TaskScheduler, configurator.ReceiveSelfPublish);

            return new Subscriber(model, _exchange, queue, consumer, configurator.ReceiveSelfPublish);
        }

        public ISubscription RegisterSubscription<T>(T instance, Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = CreateConfigurator(configure);

            IModel model = _connection.CreateModel();

            QueueDeclareOk queue = CreateQueue(model, configurator);

            IMessageConsumer consumer = new MessageConsumer(model, BusId, _messageHelper, _serializerHelper, configurator.ErrorSubscriber, configurator.TaskScheduler, configurator.ReceiveSelfPublish);

            return new Subscription(model, _exchange, queue, consumer, instance, configurator.ReceiveSelfPublish);
        }

        private static SubscriberConfigurator CreateConfigurator(Action<ISubscriberConfigurator> configure)
        {
            SubscriberConfigurator configurator = new SubscriberConfigurator();

            if (configure != null)
            {
                configure(configurator);
            }

            return configurator;
        }

        private static QueueDeclareOk CreateQueue(IModel model, SubscriberConfigurator configurator)
        {
            string queueName = configurator.QueueName;

            bool durable = !string.IsNullOrEmpty(queueName);
            bool exclusive = !durable;
            bool autoDelete = !durable;

            QueueDeclareOk queueDeclare = model.QueueDeclare(queueName, durable, exclusive, autoDelete, new Dictionary<string, object>());

            return queueDeclare;
        } 
    }
}
