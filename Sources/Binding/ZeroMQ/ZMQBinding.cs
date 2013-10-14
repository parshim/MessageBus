using System.ServiceModel.Channels;
using System.Xml;

namespace MessageBus.Binding.ZeroMQ
{
    public abstract class ZMQBinding : System.ServiceModel.Channels.Binding
    {
        protected ZMQBinding(string name, SocketMode socketMode)
        {
            Name = name;
            SocketMode = socketMode;
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BinaryMessageEncodingBindingElement encoding = new BinaryMessageEncodingBindingElement();

            ZMQTransportBindingElement transport = new ZMQTransportBindingElement(Scheme, SocketMode);

            if (ReaderQuotas != null)
            {
                ReaderQuotas.CopyTo(encoding.ReaderQuotas);
            }
            
            BindingElementCollection collection = new BindingElementCollection
                {
                    encoding, 
                    transport
                };

            return collection;
        }

        /// <summary>
        /// ZMQ Socket Mode
        /// </summary>
        public SocketMode SocketMode { get; set; }
       
        /// <summary>
        /// Serializer quotas
        /// </summary>
        public XmlDictionaryReaderQuotas ReaderQuotas { get; set; }
    }
}