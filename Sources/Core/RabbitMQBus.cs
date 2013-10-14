using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class RabbitMQBus : Bus
    {
        protected readonly string _host;
        protected readonly string _exchange;
        protected readonly RabbitMQBinding _binding;
        protected readonly IErrorSubscriber _errorSubscriber;

        private IChannelFactory<IOutputChannel> _channelFactory;

        public RabbitMQBus(string busId = null, string host = "localhost", string exchange = "amq.headers", 
                            bool exactlyOnce = false, MessageFormat messageFormat = MessageFormat.Text, IErrorSubscriber errorSubscriber = null, XmlDictionaryReaderQuotas readerQuotas = null)
            : base(busId)
        {
            _host = host;
            _exchange = exchange;

            _errorSubscriber = errorSubscriber ?? new NullErrorSubscriber();

            _binding = new RabbitMQBinding
                {
                    ApplicationId = busId,
                    OneWayOnly = true,
                    ExactlyOnce = exactlyOnce,
                    PersistentDelivery = false,
                    HeaderNamespace = MessagingConstancts.Namespace.MessageBus,
                    MessageFormat = messageFormat,
                    ReaderQuotas = readerQuotas
                };
        }

        protected virtual IOutputChannel CreateOutputChannel(BufferManager bufferManager)
        {
            if (_channelFactory == null)
            {
                object[] parameters = CreateParameters(bufferManager);

                _channelFactory = _binding.BuildChannelFactory<IOutputChannel>(parameters);

                _channelFactory.Open();
            }

            Uri toAddress = new Uri(string.Format("amqp://{0}/{1}", _host, _exchange));

            return _channelFactory.CreateChannel(new EndpointAddress(toAddress));
        }

        protected virtual IInputChannel CreateInputChannel(BufferManager bufferManager)
        {
            Uri listenUriBaseAddress = new Uri(string.Format("amqp://{0}/", _host));

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

        public override IPublisher CreatePublisher(BufferManager bufferManager = null)
        {
            IOutputChannel outputChannel = CreateOutputChannel(bufferManager);

            return new Publisher(outputChannel, _binding.MessageVersion, BusId);
        }

        public override ISubscriber CreateSubscriber(BufferManager bufferManager = null)
        {
            RabbitMQTransportInputChannel inputChannel = CreateInputChannel(bufferManager) as RabbitMQTransportInputChannel;

            if (inputChannel == null)
            {
                throw new NoIncomingConnectionAcceptedException();
            }

            return new RabbitMQSubscriber(inputChannel, _exchange, BusId, _errorSubscriber);
        }
    }
}