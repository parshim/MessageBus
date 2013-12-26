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

        public RabbitMQBus(string busId = null, string host = null, int? port = null, string exchange = null, 
                            bool exactlyOnce = false, MessageFormat messageFormat = MessageFormat.Text, XmlDictionaryReaderQuotas readerQuotas = null, bool mandatory = false)
            : base(busId)
        {
            SetPropertyValues(host, port, exchange);

            _binding = new RabbitMQBinding
                {
                    ApplicationId = busId,
                    OneWayOnly = true,
                    ExactlyOnce = exactlyOnce,
                    PersistentDelivery = false,
                    HeaderNamespace = MessagingConstancts.Namespace.MessageBus,
                    MessageFormat = messageFormat,
                    ReaderQuotas = readerQuotas,
                    Mandatory = mandatory
                };
        }

        private void SetPropertyValues(string host = null, int? port = null, string exchange = null)
        {
            RabbitMQBusConfigSectionHandler section = ConfigurationManager.GetSection(RabbitMQBusConfigSectionHandler.SectionName) as RabbitMQBusConfigSectionHandler;

            _host = host ?? (section == null ? "localhost" : section.BrokerHost);
            _port = port ?? (section == null ? 5672 : section.Port);
            _exchange = exchange ?? (section == null ? "amq.headers" : section.Exchange);
        }

        protected virtual IOutputChannel CreateOutputChannel(BufferManager bufferManager, IFaultMessageProcessor messageProcessor)
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

        protected virtual IInputChannel CreateInputChannel(BufferManager bufferManager)
        {
            Uri listenUriBaseAddress = new Uri(string.Format("amqp://{0}:{1}/", _host, _port));

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
            FaultMessageProcessor faultMessageProcessor = configurator.FaultMessageProcessor;

            RabbitMQTransportOutputChannel outputChannel = CreateOutputChannel(configurator.BufferManager, faultMessageProcessor) as RabbitMQTransportOutputChannel;

            if (outputChannel == null)
            {
                throw new NoIncomingConnectionAcceptedException();
            }

            return new Publisher(outputChannel, _binding.MessageVersion, faultMessageProcessor, BusId);
        }

        internal override ISubscriber OnCreateSubscriber(SubscriberConfigurator configurator)
        {
            RabbitMQTransportInputChannel inputChannel = CreateInputChannel(configurator.BufferManager) as RabbitMQTransportInputChannel;

            if (inputChannel == null)
            {
                throw new NoIncomingConnectionAcceptedException();
            }

            var messageFilter = new RabbitMQMessageFilter(inputChannel, _exchange);

            return new Subscriber(inputChannel, BusId, configurator.ErrorSubscriber, messageFilter);
        }
    }
}