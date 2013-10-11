using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQChannelListener<TChannel> : ChannelListenerBase<TChannel> where TChannel : class, IChannel
    {
        private readonly Context _context = new Context();
        private readonly BindingContext _bindingContext;
        private readonly SocketMode _socketMode;
        private readonly EndpointAddress _address;

        private readonly Action<TimeSpan> _onCloseHandler;
        private readonly Action<TimeSpan> _onOpenHandler;
        private readonly Func<TimeSpan, bool> _onWaitForChannelHandler;

        private ChannelBase _channel;

        public ZMQChannelListener(BindingContext bindingContext, SocketMode socketMode)
        {
            _bindingContext = bindingContext;
            _socketMode = socketMode;

            if (bindingContext.ListenUriMode == ListenUriMode.Explicit && bindingContext.ListenUriBaseAddress != null)
            {
                _address = new EndpointAddress(new Uri(bindingContext.ListenUriBaseAddress, bindingContext.ListenUriRelativeAddress));
            }
            else
            {
                _address = new EndpointAddress(string.Format("{0}://127.0.0.1:{1}", bindingContext.Binding.Scheme, 2020));
            }

            _onCloseHandler = OnClose;
            _onOpenHandler = OnOpen;
            _onWaitForChannelHandler = OnWaitForChannel;
        }

        protected override void OnAbort()
        {
            OnClose(TimeSpan.Zero);
        }
        
        protected override void OnClose(TimeSpan timeout)
        {
        }
        
        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return false;
        }

        public override Uri Uri
        {
            get { return _address.Uri; }
        }

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            // Single channel implementation 
            if (_channel != null)
            {
                return null;
            }

            Socket socket = CreateSocket();

            if (typeof(TChannel) == typeof(IInputChannel))
            {
                _channel = new ZMQInputChannel(this, _bindingContext, _context, socket, _address, _socketMode);
            }

            if (_channel == null)
            {
                throw new System.Exception("Channel not supported");
            }

            return (TChannel)(object)_channel;
        }

        private Socket CreateSocket()
        {
            SocketType type;

            switch (_socketMode)
            {
                case SocketMode.PubSub:
                    type = SocketType.SUB;
                    break;
                case SocketMode.PushPull:
                    type = SocketType.PULL;
                    break;
                default:
                    type = SocketType.REP;
                    break;
            }

            return _context.Socket(type);
        }

        #region Async Methods
        
        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _onCloseHandler.BeginInvoke(timeout, callback, state);
        }

        protected override void OnOpen(TimeSpan timeout)
        {}

        protected override void OnEndClose(IAsyncResult result)
        {
            _onCloseHandler.EndInvoke(result);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _onOpenHandler.BeginInvoke(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            _onOpenHandler.EndInvoke(result);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _onWaitForChannelHandler.BeginInvoke(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return _onWaitForChannelHandler.EndInvoke(result);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OnAcceptChannelAsyncResult<TChannel>
                {
                    AsyncState = state,
                    Timeout = timeout,
                    Channel = OnAcceptChannel(timeout)
                };
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            OnAcceptChannelAsyncResult<TChannel> asyncResult = (OnAcceptChannelAsyncResult<TChannel>) result;
            
            if (asyncResult.IsCompleted)
            {
                return asyncResult.Channel;
            }

            if (asyncResult.Wait())
            {
                return asyncResult.Channel;
            }

            return null;
        }

        #endregion
    }

    public class OnAcceptChannelAsyncResult<TChannel> : IAsyncResult
    {
        private readonly WaitHandle _handle = new ManualResetEvent(false);

        public bool IsCompleted
        {
            get { return Channel != null; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _handle; }
        }

        public object AsyncState { get; set; }
        
        public TimeSpan Timeout { get; set; }

        public TChannel Channel { get; set; }

        public bool CompletedSynchronously
        {
            get { return IsCompleted; }
        }

        public bool Wait()
        {
            return _handle.WaitOne(Timeout);
        }
    }
}