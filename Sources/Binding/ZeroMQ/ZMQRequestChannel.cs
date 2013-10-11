using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQRequestChannel : ZMQChannelBase, IRequestChannel
    {
        private readonly EndpointAddress _address;
        private readonly Uri _via;

        public ZMQRequestChannel(ChannelManagerBase channelManager, BindingContext context, Socket socket, EndpointAddress address, Uri via, SocketMode socketMode) 
            : base(channelManager, context, socket, socketMode)
        {
            _via = via;
            _address = address;
        }
        
        protected override void OnOpen(TimeSpan timeout)
        {
            _socket.Bind(_address.Uri.ToString().TrimEnd('/'));
        }
        
        protected override void OnClose(TimeSpan timeout)
        {
            _socket.Dispose();
        }

        public Message Request(Message message)
        {
            return Request(message, TimeSpan.MaxValue);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            Send(message);

            // TODO: use timeout
            byte[] bytes = Socket.Recv();

            Message reply = ConstructMessage(bytes);

            reply.Headers.From = _address;

            return reply;
        }
        
        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return BeginRequest(message, TimeSpan.MaxValue, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task<Message>.Factory.StartNew(s => Request(message, timeout), state).ContinueWith(task => callback(task));
        }

        public Message EndRequest(IAsyncResult result)
        {
            Task<Message> task = (Task<Message>)result;

            task.Wait();

            return task.Result;
        }

        public EndpointAddress RemoteAddress { get { return _address; } }

        public Uri Via { get { return _via; } }
    }
}