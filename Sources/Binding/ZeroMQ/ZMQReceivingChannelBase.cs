using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public abstract class ZMQReceivingChannelBase : ZMQChannelBase
    {
        private delegate bool TryReceiveHandler(TimeSpan timeout, out Message message);
        
        private readonly Func<TimeSpan, Message> _onReceiveHandler;
        private readonly TryReceiveHandler _onTryReceiveHandler;
        private readonly Func<TimeSpan, bool> _onWaitForMessageHandler;

        private readonly Context _context;

        protected ZMQReceivingChannelBase(ChannelManagerBase channelManager, BindingContext bindingContext, Context context, Socket socket, SocketMode socketMode)
            : base(channelManager, bindingContext, socket, socketMode)
        {
            _onReceiveHandler = Receive;
            _onTryReceiveHandler = TryReceive;
            _onWaitForMessageHandler = WaitForMessage;
            _context = context;
        }

        public abstract EndpointAddress LocalAddress { get; }

        #region Receive

        public Message Receive()
        {
            return Receive(TimeSpan.MaxValue);
        }

        public Message Receive(TimeSpan timeout)
        {
            byte[] bytes;
            int length;

            try
            {
                byte[] lengthBytes = _socket.Recv();
                
                length = BitConverter.ToInt32(lengthBytes, 0);

                if (_bufferManager == null)
                {
                    bytes = _socket.Recv();
                }
                else
                {
                    bytes = _bufferManager.TakeBuffer(length);

                    _socket.Recv(bytes, out length);
                }
            }
            catch
            {
                return null;
            }

            if (bytes == null) return null;

            Message message;

            if (_bufferManager == null)
            {
                message = ConstructMessage(bytes);
            }
            else
            {
                message = ConstructMessage(new ArraySegment<byte>(bytes, 0, length), _bufferManager);
            }

            message.Headers.To = LocalAddress.Uri;

            return message;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = Receive(timeout);

            return message != null;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return true;
        }

        #endregion


        protected override void OnClose(TimeSpan timeout)
        {
            _socket.Dispose();
            _context.Dispose();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            string addr = LocalAddress.Uri.ToString().TrimEnd('/');

            _socket.Bind(addr);

            if (SocketMode == SocketMode.PubSub)
            {
                _socket.Subscribe(new byte[0]);
            }
        }

        #region Async Receive
        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return _onReceiveHandler.BeginInvoke(Context.Binding.ReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _onReceiveHandler.BeginInvoke(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return _onReceiveHandler.EndInvoke(result);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Message message;
            return _onTryReceiveHandler.BeginInvoke(timeout, out message, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return _onTryReceiveHandler.EndInvoke(out message, result);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _onWaitForMessageHandler.BeginInvoke(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return _onWaitForMessageHandler.EndInvoke(result);
        }
        #endregion



    }
}