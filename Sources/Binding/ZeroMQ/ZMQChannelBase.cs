using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public abstract class ZMQChannelBase : ChannelBase
    {
        protected readonly MessageEncoder _encoder;
        private readonly BindingContext _context;
        protected readonly Socket _socket;
        protected readonly BufferManager _bufferManager;

        private readonly Action<Message> _onSendHandler;
        private readonly SocketMode _socketMode;

        protected ZMQChannelBase(ChannelManagerBase channelManager, BindingContext context, Socket socket, SocketMode socketMode)
            : base(channelManager)
        {
            MessageEncodingBindingElement encoderElem = context.BindingParameters.Find<MessageEncodingBindingElement>();
            
            _encoder = encoderElem.CreateMessageEncoderFactory().Encoder;
            
            _context = context;
            _socket = socket;
            _socketMode = socketMode;

            _onSendHandler = Send;

            _bufferManager = context.BindingParameters.Find<BufferManager>();
        }

        protected BindingContext Context
        {
            get { return _context; }
        }

        protected Socket Socket
        {
            get { return _socket; }
        }
        
        protected SocketMode SocketMode
        {
            get { return _socketMode; }
        }

        protected Message ConstructMessage(byte[] buffer)
        {
            Message message = _encoder.ReadMessage(new MemoryStream(buffer), 8192);
            
            return message;
        }
        
        protected Message ConstructMessage(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            Message message = _encoder.ReadMessage(buffer, bufferManager);
            
            return message;
        }

        #region Send

        public void Send(Message message, TimeSpan timeout)
        {
            Send(message);
        }

        public void Send(Message message)
        {
            byte[] body;

            using (MemoryStream str = new MemoryStream())
            {
                _encoder.WriteMessage(message, str);
                body = str.ToArray();
            }

            byte[] lengthBytes = BitConverter.GetBytes(body.Length);

            _socket.SendMore(lengthBytes);
            _socket.Send(body);
        }

        #endregion
        
        #region Async Send

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _onSendHandler.BeginInvoke(message, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return _onSendHandler.BeginInvoke(message, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            _onSendHandler.EndInvoke(result);
        }

        #endregion

        #region Abort \ Close

        protected override void OnAbort()
        {
            OnClose(TimeSpan.Zero);
        }
        
        #endregion

        #region Asyn Open\Close

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(s => OnClose(timeout), state).ContinueWith(task => callback(task));
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            Task.Factory.FromAsync(result, asyncResult => { }).Wait();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(s => OnOpen(timeout), state).ContinueWith(task => callback(task));
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            Task.Factory.FromAsync(result, asyncResult => { }).Wait();
        }

        #endregion
    }
}