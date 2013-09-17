using System;
using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
    public abstract class RabbitMQChannelListenerBase<TChannel> : ChannelListenerBase<TChannel> where TChannel: class, IChannel
    {
        private readonly Uri _listenUri;
        private readonly BindingContext _context;
        private readonly CommunicationOperation _closeMethod;
        private readonly CommunicationOperation _openMethod;
        private readonly CommunicationOperation<TChannel> _acceptChannelMethod;
        private readonly CommunicationOperation<bool> _waitForChannelMethod;

        protected RabbitMQTransportBindingElement _bindingElement;

        protected RabbitMQChannelListenerBase(BindingContext context, Uri listenUri)
        {
            _context = context;
            _listenUri = listenUri;
            _bindingElement = context.Binding.Elements.Find<RabbitMQTransportBindingElement>();
            _closeMethod = OnClose;
            _openMethod = OnOpen;
            _waitForChannelMethod = OnWaitForChannel;
            _acceptChannelMethod = OnAcceptChannel;
        }
        
        protected override void OnAbort()
        {
            OnClose(_context.Binding.CloseTimeout);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _acceptChannelMethod.BeginInvoke(timeout, callback, state);
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return _acceptChannelMethod.EndInvoke(result);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _waitForChannelMethod.BeginInvoke(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return _waitForChannelMethod.EndInvoke(result);
        }
        
        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _closeMethod.BeginInvoke(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _openMethod.BeginInvoke(timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            _closeMethod.EndInvoke(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            _openMethod.EndInvoke(result);
        }
            
        
        public override Uri Uri
        {
            get { return _listenUri; }
        }

        protected BindingContext Context
        {
            get { return _context; }
        }
    }
}
