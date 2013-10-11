using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQReplyChannel : ZMQReceivingChannelBase, IReplyChannel
    {
        private readonly EndpointAddress _address;

        public ZMQReplyChannel(ChannelManagerBase channelManager, BindingContext bindingContext, Context context, Socket socket, EndpointAddress address, SocketMode socketMode)
            : base(channelManager, bindingContext, context, socket, socketMode)
        {
            _address = address;
        }
        
        public RequestContext ReceiveRequest()
        {
            return ReceiveRequest(TimeSpan.MaxValue);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            Message message = Receive(timeout);

            if (message == null) return null;

            return new ZMQRequestContext(message, this);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            Message message;

            bool receive = TryReceive(timeout, out message);

            context = receive ? new ZMQRequestContext(message, this) : null;

            return receive;
        }
        
        public bool WaitForRequest(TimeSpan timeout)
        {
            return WaitForMessage(timeout);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return BeginReceiveRequest(TimeSpan.MaxValue, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task<RequestContext>.Factory.StartNew(s => ReceiveRequest(timeout), state).ContinueWith(task => callback(task));
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            Task<RequestContext> task = (Task<RequestContext>) result;

            task.Wait();

            return task.Result;
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task<RequestContext>.Factory.StartNew(s => ReceiveRequest(timeout), state)
                                       .ContinueWith(task => callback(task));
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            Task<RequestContext> task = (Task<RequestContext>) result;

            task.Wait();

            context = task.Result;

            return context != null;
        }
        
        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task<bool>.Factory.StartNew(s => WaitForRequest(timeout), state).ContinueWith(task => callback(task));
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            Task<bool> task = (Task<bool>) result;

            task.Wait();

            return task.Result;
        }

        public override EndpointAddress LocalAddress { get { return _address; } }
    }
}