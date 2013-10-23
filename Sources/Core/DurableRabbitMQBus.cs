using System;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Binding.RabbitMQ;

namespace MessageBus.Core
{
    public class DurableRabbitMQBus : RabbitMQBus
    {
        private readonly string _queue;

        private IChannelListener<IInputChannel> _listener;
        
        public DurableRabbitMQBus(string queue, string busId = null, string host = "localhost", string exchange = "amq.headers",
                            bool exactlyOnce = false, MessageFormat messageFormat = MessageFormat.Text, XmlDictionaryReaderQuotas readerQuotas = null, bool mandatory = false)
            : base(busId, host, exchange, exactlyOnce, messageFormat, readerQuotas, mandatory)
        {
            _queue = queue;
        }

        protected override IInputChannel CreateInputChannel(BufferManager bufferManager)
        {
            if (_listener == null)
            {
                Uri listenUriBaseAddress = new Uri(string.Format("amqp://{0}/{1}", _host, _queue));

                object[] parameters = CreateParameters(bufferManager);

                _listener = _binding.BuildChannelListener<IInputChannel>(listenUriBaseAddress, parameters);

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