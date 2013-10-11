using System.ServiceModel.Channels;
using System.Xml;

namespace MessageBus.Binding.ZeroMQ
{
    public abstract class ZMQBinding : System.ServiceModel.Channels.Binding
    {
        // Stack
        private readonly MessageEncodingBindingElement _encoding;
        private readonly ZMQTransportBindingElement _transport;

        protected ZMQBinding(string name, SocketMode socketMode)
        {
            Name = name;
            Namespace = "http://schemas.a-solutions.com/2013/ZMQ/";

            _encoding = new BinaryMessageEncodingBindingElement
                {
                    ReaderQuotas = new XmlDictionaryReaderQuotas
                        {
                            MaxArrayLength = 10 * 1024 * 1024
                        }
                };

            _transport = new ZMQTransportBindingElement(Scheme, socketMode);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection collection = new BindingElementCollection
                {
                    _encoding, 
                    _transport
                };

            return collection;
        }

        public SocketMode SocketMode
        {
            get { return _transport.SocketMode; }
            set { _transport.SocketMode = value; }
        }

    }
}