using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public abstract class ZMQDuplexChannel : ZMQReceivingChannelBase, IDuplexChannel
    {
        protected readonly EndpointAddress _address;
        private readonly Uri _via;

        protected ZMQDuplexChannel(ChannelManagerBase channelManager, BindingContext bindingContext, Context context, Socket socket, SocketMode socketMode, EndpointAddress address, Uri via) 
            : base(channelManager, bindingContext, context, socket, socketMode)
        {
            _via = via;
            _address = address;
        }
        
        public EndpointAddress RemoteAddress { get { return _address; } }

        public override EndpointAddress LocalAddress { get { return _address; } }

        public Uri Via { get { return _via; } }
    }

    public class ZMQServerDuplexChannel : ZMQDuplexChannel
    {
        public ZMQServerDuplexChannel(ChannelManagerBase channelManager, BindingContext bindingContext, Context context, Socket socket, SocketMode socketMode, EndpointAddress address, Uri via)
            : base(channelManager, bindingContext, context, socket, socketMode, address, via)
        {
        }

    }
    
    public class ZMQClientDuplexChannel : ZMQDuplexChannel
    {
        public ZMQClientDuplexChannel(ChannelManagerBase channelManager, BindingContext bindingContext, Context context, Socket socket, SocketMode socketMode, EndpointAddress address, Uri via)
            : base(channelManager, bindingContext, context, socket, socketMode, address, via)
        {
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            _socket.Bind(_address.Uri.ToString().TrimEnd('/'));
        }

        protected override void OnClose(TimeSpan timeout)
        {
            base.OnClose(timeout);

            _socket.Dispose();
        }
    }
}