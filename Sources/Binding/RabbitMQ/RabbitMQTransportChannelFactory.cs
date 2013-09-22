using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
    internal sealed class RabbitMQTransportChannelFactory<T> : ChannelFactoryBase<T>
    {
        private readonly BindingContext _context;
        private readonly CommunicationOperation _openMethod;
        
        public RabbitMQTransportChannelFactory(BindingContext context)
        {
            _context = context;
            _openMethod = Open;
        }

        protected override T OnCreateChannel(EndpointAddress address, Uri via)
        {
            IChannel channel;

            if (typeof (T) == typeof (IOutputChannel))
            {    
                channel = new RabbitMQTransportOutputChannel(_context, address, via);
            }
            else
            {
                return default(T);
            }

            return (T) channel;
        }
        
        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _openMethod.BeginInvoke(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            _openMethod.EndInvoke(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {

        }

        protected override void OnClose(TimeSpan timeout)
        {

        }

        protected override void OnAbort()
        {
            base.OnAbort();
            OnClose(_context.Binding.CloseTimeout);
        }
    }
}
