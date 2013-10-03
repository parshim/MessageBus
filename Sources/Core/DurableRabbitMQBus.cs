using System;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class DurableRabbitMQBus : RabbitMQBus
    {
        private readonly string _queue;

        private IChannelListener<IInputChannel> _listener;

        public DurableRabbitMQBus(string queue)
            : this(Guid.NewGuid().ToString(), queue)
        {
        }

        public DurableRabbitMQBus(string busId, string queue, string host = "localhost", string exchange = "amq.fanout", bool exactlyOnce = false, IErrorSubscriber errorSubscriber = null) : base(busId, host, exchange, exactlyOnce, errorSubscriber)
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