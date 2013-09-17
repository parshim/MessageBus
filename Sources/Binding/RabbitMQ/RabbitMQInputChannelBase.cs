using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
    internal abstract class RabbitMQInputChannelBase : RabbitMQChannelBase, IInputChannel
    {
        private readonly EndpointAddress _localAddress;
        private readonly CommunicationOperation<Message> _receiveMethod;
        private readonly CommunicationOperation<bool, Message> _tryReceiveMethod;
        private readonly CommunicationOperation<bool> _waitForMessage;
        
        protected RabbitMQInputChannelBase(BindingContext context, EndpointAddress localAddress)
        :base(context)
        {
            _localAddress = localAddress;
            _receiveMethod = Receive;
            _tryReceiveMethod = TryReceive;
            _waitForMessage = WaitForMessage;
        }


        #region Async Methods
        public virtual IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _receiveMethod.BeginInvoke(timeout, callback, state);
        }

        public virtual IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return _receiveMethod.BeginInvoke(Context.Binding.ReceiveTimeout, callback, state);
        }

        public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Message message;
            return _tryReceiveMethod.BeginInvoke(timeout, out message, callback, state);
        }

        public virtual IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _waitForMessage.BeginInvoke(timeout, callback, state);
        }

        public virtual Message EndReceive(IAsyncResult result)
        {
            return _receiveMethod.EndInvoke(result);
        }

        public virtual bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return _tryReceiveMethod.EndInvoke(out message, result);
        }

        public virtual bool EndWaitForMessage(IAsyncResult result)
        {
            return _waitForMessage.EndInvoke(result);
        }
        #endregion

        public abstract Message Receive(TimeSpan timeout);

        public abstract bool TryReceive(TimeSpan timeout, out Message message);

        public abstract bool WaitForMessage(TimeSpan timeout);

        public virtual Message Receive()
        {
            return Receive(Context.Binding.ReceiveTimeout);
        }
        
        public EndpointAddress LocalAddress
        {
            get { return _localAddress; }
        }
    }
}
