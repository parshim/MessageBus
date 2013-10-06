using System;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class DurableRabbitMQBus : RabbitMQBus
    {
        private readonly string _queue;

        private IChannelListener<IInputChannel> _listener;
        
        public DurableRabbitMQBus(string queue, string busId = null, string host = "localhost", string exchange = "amq.headers",
                            bool exactlyOnce = false, MessageFormat messageFormat = MessageFormat.Text, IErrorSubscriber errorSubscriber = null, XmlDictionaryReaderQuotas readerQuotas = null)
            : base(busId, host, exchange, exactlyOnce, messageFormat, errorSubscriber, readerQuotas)
        {
            _queue = queue;
        }

        protected override IInputChannel CreateInputChannel()
        {
            if (_listener == null)
            {
                Uri listenUriBaseAddress = new Uri(string.Format("amqp://{0}/{1}", _host, _queue));

                _listener = _binding.BuildChannelListener<IInputChannel>(listenUriBaseAddress);

                _listener.Open();
            }

            return _listener.AcceptChannel();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_listener != null)
            {
                _listener.Close();
            }
        }
    }
}