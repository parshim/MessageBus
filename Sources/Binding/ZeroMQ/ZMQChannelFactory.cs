using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
    {
        private readonly Context _context = new Context();
        private readonly BindingContext _bindingContext;
        private readonly SocketMode _socketMode;

        public ZMQChannelFactory(BindingContext bindingContext, SocketMode socketMode)
        {
            _bindingContext = bindingContext;
            _socketMode = socketMode;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            _context.Dispose();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(s => OnOpen(timeout), state).ContinueWith(task => callback(task));
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            Task.Factory.FromAsync(result, asyncResult => {}).Wait();
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            Socket socket = CreateSocket();

            if (typeof (TChannel) == typeof (IOutputChannel))
            {
                object channel = new ZMQOutputChannel(this, _bindingContext, socket, _socketMode, address, via);

                return (TChannel) channel;
            }

            throw new System.Exception("Channel not supported");
        }

        private Socket CreateSocket()
        {
            SocketType type;

            switch (_socketMode)
            {
                case SocketMode.PubSub:
                    type = SocketType.PUB;
                    break;
                case SocketMode.PushPull:
                    type = SocketType.PUSH;
                    break;
                default:
                    type = SocketType.REQ;
                    break;
            }

            return _context.Socket(type);
        }
    }
}