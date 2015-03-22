using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class RabbitMQBus : Bus
    {
        protected string _host;
        protected int _port;
        protected string _exchange;

        protected readonly RabbitMQBinding _binding;

        private IChannelFactory<IOutputChannel> _channelFactory;

        public RabbitMQBus() : this(null, null, 0, null, false, MessageFormat.Text, null, false)
        {
            
        }

        public RabbitMQBus(string busId = null, string host = null, int port = 0, string exchange = null, 
                            bool exactlyOnce = false, MessageFormat messageFormat = MessageFormat.Text, XmlDictionaryReaderQuotas readerQuotas = null, bool mandatory = false)
            : base(busId)
        {
            RabbitMQBusConfigSectionHandler section = ConfigurationManager.GetSection(RabbitMQBusConfigSectionHandler.SectionName) as RabbitMQBusConfigSectionHandler;

            _host = GetPropertyValue(host, "localhost", section, s => s.BrokerHost);
            _port = GetPropertyValue(port, 5672, section, s => s.Port);
            _exchange = GetPropertyValue(exchange, "amq.headers", section, s => s.Exchange);

            readerQuotas = GetPropertyValue(readerQuotas, null, section, s => new XmlDictionaryReaderQuotas
                {
                    MaxArrayLength = s.ReaderQuotas.MaxArrayLength,
                    MaxBytesPerRead = s.ReaderQuotas.MaxBytesPerRead,
                    MaxDepth = s.ReaderQuotas.MaxDepth,
                    MaxNameTableCharCount = s.ReaderQuotas.MaxNameTableCharCount,
                    MaxStringContentLength = s.ReaderQuotas.MaxStringContentLength
                });

            _binding = new RabbitMQBinding
                {
                    ApplicationId = busId,
                    OneWayOnly = true,
                    ExactlyOnce = exactlyOnce,
                    PersistentDelivery = false,
                    HeaderNamespace = MessagingConstants.Namespace.MessageBus,
                    MessageFormat = messageFormat,
                    ReaderQuotas = readerQuotas,
                    Mandatory = mandatory
                };
        }

        protected T GetPropertyValue<T>(T value, T defaultValue, RabbitMQBusConfigSectionHandler section, Func<RabbitMQBusConfigSectionHandler, T> selector)
        {
            if (!Equals(value, default(T))) return value;

            if (section == null) return defaultValue;

            return selector(section);
        }

        private IOutputChannel CreateOutputChannel(BufferManager bufferManager, IFaultMessageProcessor messageProcessor)
        {
            if (_channelFactory == null)
            {
                object[] parameters = CreateParameters(bufferManager, messageProcessor);

                _channelFactory = _binding.BuildChannelFactory<IOutputChannel>(parameters);

                _channelFactory.Open();
            }

            Uri toAddress = new Uri(string.Format("amqp://{0}:{1}/{2}", _host, _port, _exchange));

            return _channelFactory.CreateChannel(new EndpointAddress(toAddress));
        }

        private IInputChannel CreateInputChannel(BufferManager bufferManager, string queueName)
        {
            Uri listenUriBaseAddress = new Uri(string.Format("amqp://{0}:{1}/{2}", _host, _port, queueName));

            object[] parameters = CreateParameters(bufferManager);

            IChannelListener<IInputChannel> listener = _binding.BuildChannelListener<IInputChannel>(listenUriBaseAddress, parameters);

            listener.Open();

            try
            {
                return listener.AcceptChannel();
            }
            finally
            {
                listener.Close();
            }
        }

        public override void Dispose()
        {
            if (_channelFactory != null)
            {
                _channelFactory.Close();
            }
        }

        internal override IPublisher OnCreatePublisher(PublisherConfigurator configurator)
        {
            IKnownContractCollector collector = new KnownContractCollector();

            IFaultMessageProcessor faultMessageProcessor = new FaultMessageProcessor(configurator.ErrorHandler, collector);

            RabbitMQTransportOutputChannel outputChannel = CreateOutputChannel(configurator.BufferManager, faultMessageProcessor) as RabbitMQTransportOutputChannel;

            if (outputChannel == null)
            {
                throw new NoIncomingConnectionAcceptedException();
            }

            return new Publisher(outputChannel, _binding.MessageVersion, collector, BusId);
        }

        internal override IInputChannel OnCreateInputChannel(SubscriberConfigurator configurator)
        {
            IInputChannel inputChannel = CreateInputChannel(configurator.BufferManager, configurator.QueueName);

            if (inputChannel == null)
            {
                throw new NoIncomingConnectionAcceptedException();
            }

            return inputChannel;
        }

        internal override IMessageFilter OnCreateMessageFilter(IInputChannel channel)
        {
            RabbitMQTransportInputChannel inputChannel = channel as RabbitMQTransportInputChannel;

            return new RabbitMQMessageFilter(inputChannel, _exchange);
        }

    }
}