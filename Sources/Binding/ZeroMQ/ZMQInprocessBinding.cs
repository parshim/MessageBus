using ZMQ;

namespace MessageBus.Binding.ZeroMQ
{
    public sealed class ZMQInprocessBinding : ZMQBinding
    {
        public ZMQInprocessBinding()
            : base("ZMQInprocessBinding", SocketMode.PubSub)
        {
        }

        public override string Scheme
        {
            get { return "inproc"; }
        }

    }
}