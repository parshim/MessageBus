using System.ServiceModel.Channels;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQTransportBindingElement : TransportBindingElement
    {
        private readonly string _scheme;
        private SocketMode _socketMode;
        
        public ZMQTransportBindingElement(string scheme, SocketMode socketMode)
        {
            _scheme = scheme;
            _socketMode = socketMode;
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IOutputChannel);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IInputChannel);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return new ZMQChannelFactory<TChannel>(context, _socketMode);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            return new ZMQChannelListener<TChannel>(context, _socketMode);
        }

        public override BindingElement Clone()
        {
            return new ZMQTransportBindingElement(_scheme, _socketMode);
        }
        
        public override string Scheme
        {
            get { return _scheme; }
        }

        public SocketMode SocketMode
        {
            get { return _socketMode; }
            set { _socketMode = value; }
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }
    }
}