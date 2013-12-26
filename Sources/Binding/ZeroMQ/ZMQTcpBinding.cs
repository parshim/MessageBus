namespace MessageBus.Binding.ZeroMQ
{
    public sealed class ZMQTcpBinding : ZMQBinding
    {
        public ZMQTcpBinding()
            : base("ZMQTcpBinding", SocketMode.PubSub)
        {
        }

        public override string Scheme
        {
            get { return "tcp"; }
        }
    }
    
    public sealed class ZMQIpcBinding : ZMQBinding
    {
        public ZMQIpcBinding()
            : base("ZMQIpcBinding", SocketMode.PubSub)
        {
        }

        public override string Scheme
        {
            get { return "ipc"; }
        }
    }
}