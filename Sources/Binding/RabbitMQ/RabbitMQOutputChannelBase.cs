using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
    internal abstract class RabbitMQOutputChannelBase : RabbitMQChannelBase, IOutputChannel
    {
        private readonly SendOperation _sendMethod;
        private readonly EndpointAddress _address;
        private readonly Uri _via;

        protected RabbitMQOutputChannelBase(BindingContext context, EndpointAddress address, Uri via)
            : base(context)
        {
            _address = address;
            _via = via;
            _sendMethod = Send;
        }
        
        #region Async Methods

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _sendMethod.BeginInvoke(message, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return _sendMethod.BeginInvoke(message, Context.Binding.SendTimeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            _sendMethod.EndInvoke(result);
        }

        #endregion

        public abstract void Send(Message message, TimeSpan timeout);

        public virtual void Send(Message message)
        {
            Send(message, Context.Binding.SendTimeout);
        }

        public EndpointAddress RemoteAddress
        {
            get { return _address; }
        }

        public Uri Via
        {
            get { return _via; }
        }
    }
}
