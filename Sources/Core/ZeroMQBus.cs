using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Binding.ZeroMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class ZeroMQBus : Bus, IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        protected readonly ZMQTcpBinding _binding;
        protected readonly IErrorSubscriber _errorSubscriber;

        private IChannelFactory<IOutputChannel> _channelFactory;

        public ZeroMQBus(string host, int port, string busId = null, IErrorSubscriber errorSubscriber = null, XmlDictionaryReaderQuotas readerQuotas = null)
            : base(busId)
        {
            _host = host;
            _port = port;
            _binding = new ZMQTcpBinding
                {
                    SocketMode = SocketMode.PubSub,
                    ReaderQuotas = readerQuotas
                };

            _errorSubscriber = errorSubscriber ?? new NullErrorSubscriber();
        }

        public override IPublisher CreatePublisher()
        {
            if (_channelFactory == null)
            {
                _channelFactory = _binding.BuildChannelFactory<IOutputChannel>();

                _channelFactory.Open();
            }

            Uri toAddress = CreateUri();

            IOutputChannel outputChannel = _channelFactory.CreateChannel(new EndpointAddress(toAddress));

            return new Publisher(outputChannel, _binding.MessageVersion, BusId);
        }

        private Uri CreateUri()
        {
            return new Uri(string.Format("tcp://{0}:{1}", _host, _port));
        }

        public override ISubscriber CreateSubscriber()
        {
            Uri listenUriBaseAddress = CreateUri();

            IChannelListener<IInputChannel> listener = _binding.BuildChannelListener<IInputChannel>(listenUriBaseAddress);

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

            return new ZeroMQSubscriber(channel, BusId, _errorSubscriber);
        }

        public void Dispose()
        {
            if (_channelFactory != null)
            {
                _channelFactory.Close();
            }
        }
    }
}