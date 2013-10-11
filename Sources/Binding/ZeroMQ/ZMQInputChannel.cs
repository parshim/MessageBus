using System.ServiceModel;
using System.ServiceModel.Channels;
using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQInputChannel : ZMQReceivingChannelBase, IInputChannel
    {
        private readonly EndpointAddress _address;

        public ZMQInputChannel(ChannelManagerBase channelManager, BindingContext bindingContext, Context context, Socket socket, EndpointAddress address, SocketMode socketMode) 
            : base(channelManager, bindingContext, context, socket, socketMode)
        {
            _address = address;
        }
        
        public override EndpointAddress LocalAddress { get { return _address; } }

    }
}