using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQOutputChannel : ZMQChannelBase, IOutputChannel
    {
        private readonly EndpointAddress _remoteAddress;
        private readonly Uri _via;

        public ZMQOutputChannel(ChannelManagerBase channelManager, BindingContext context, Socket socket, SocketMode socketMode, EndpointAddress remoteAddress, Uri via) 
            : base(channelManager, context, socket, socketMode)
        {
            _via = via;
            _remoteAddress = remoteAddress;
        }
        
        public EndpointAddress RemoteAddress { get { return _remoteAddress; } }

        public Uri Via { get { return _via; } }

        protected override void OnOpen(TimeSpan timeout)
        {
            string addr = _remoteAddress.Uri.ToString().TrimEnd('/');

            _socket.Connect(addr);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            _socket.Dispose();
        }
    }
}