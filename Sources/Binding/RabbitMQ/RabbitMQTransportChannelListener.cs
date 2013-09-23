using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
    internal sealed class RabbitMQTransportChannelListener<T> : RabbitMQChannelListenerBase<T> where T : class, IChannel
    {
        private readonly string _bindToExchange;
        private IChannel _channel;
        
        internal RabbitMQTransportChannelListener(BindingContext context, Uri listenUri, string bindToExchange)
            : base(context, listenUri)
        {
            _bindToExchange = bindToExchange;
            _channel = null;
        }

        protected override T OnAcceptChannel(TimeSpan timeout)
        {
            // Since only one connection to a broker is required (even for communication
            // with multiple exchanges 
            if (_channel != null)
                return null;

            if (typeof (T) == typeof (IInputChannel))
            {
                _channel = new RabbitMQTransportInputChannel(Context, new EndpointAddress(Uri.ToString()), _bindToExchange);
            }
            else
            {
                return null;
            }

            _channel.Closed += ListenChannelClosed;

            return (T) _channel;
        }
        
        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return false;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void OnClose(TimeSpan timeout)
        {
#if VERBOSE
            DebugHelper.Start();
#endif  
            if (_channel != null)
            {
                _channel.Close();
                _channel = null;
            }
#if VERBOSE
            DebugHelper.Stop(" ## In.Close {{Time={0}ms}}.");
#endif
        }

        private void ListenChannelClosed(object sender, EventArgs args)
        {
            ((IInputChannel)sender).Closed -= ListenChannelClosed;

            Close();
        }
}
}
