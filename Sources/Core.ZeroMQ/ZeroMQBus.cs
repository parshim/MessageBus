using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Binding.ZeroMQ;
using MessageBus.Core;
using MessageBus.Core.API;

namespace MessageBus.Core.ZeroMQ
{
    public class ZeroMQBus : Bus
    {
        private readonly string _host;
        private readonly int _port;
        protected readonly ZMQBinding _binding;

        private IChannelFactory<IOutputChannel> _channelFactory;

        public ZeroMQBus(string host, int port, string busId = null, XmlDictionaryReaderQuotas readerQuotas = null)
            : base(busId)
        {
            _host = host;
            _port = port;
            _binding = new ZMQTcpBinding
                {
                    SocketMode = SocketMode.PubSub,
                    ReaderQuotas = readerQuotas
                };
        }

        internal override IPublisher OnCreatePublisher(PublisherConfigurator configuration)
        {
            if (_channelFactory == null)
            {
                object[] parameters = CreateParameters(configuration.BufferManager);
 
                _channelFactory = _binding.BuildChannelFactory<IOutputChannel>(parameters);

                _channelFactory.Open();
            }

            Uri toAddress = CreateUri();

            IKnownContractCollector collector = new KnownContractCollector();

            IOutputChannel outputChannel = _channelFactory.CreateChannel(new EndpointAddress(toAddress));

            return new Publisher(outputChannel, _binding.MessageVersion, collector, BusId);
        }

        private Uri CreateUri()
        {
            return new Uri(string.Format("tcp://{0}:{1}", _host, _port));
        }

        internal override ISubscriber OnCreateSubscriber(SubscriberConfigurator configuration)
        {
            Uri listenUriBaseAddress = CreateUri();

            object[] parameters = CreateParameters(configuration.BufferManager);

            IChannelListener<IInputChannel> listener = _binding.BuildChannelListener<IInputChannel>(listenUriBaseAddress, parameters);

            listener.Open();

            IInputChannel channel;

            try
            {
                channel = listener.AcceptChannel();
            }
            finally
            {
                listener.Close();
            }

            return new Subscriber(channel, BusId, configuration.ErrorSubscriber, new NullMessageFilter());
        }

        public override void Dispose()
        {
            if (_channelFactory != null)
            {
                _channelFactory.Close();
            }
        }
    }
}