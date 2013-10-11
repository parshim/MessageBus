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
}