using System;
using System.ServiceModel.Channels;
using System.Xml;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    public enum MessageFormat
    {
        Text = 0x0,
        MTOM = 0x1,
        NetBinary = 0x2,
    }

    /// <summary>
    /// A windows communication foundation binding over AMQP
    /// </summary>
    public sealed class RabbitMQBinding : System.ServiceModel.Channels.Binding
    {
        private bool _isInitialized;

        private CompositeDuplexBindingElement _duplex;
        private TextMessageEncodingBindingElement _textEncoding;
        private MtomMessageEncodingBindingElement _mtomEncoding;
        private BinaryMessageEncodingBindingElement _binaryEncoding;
        private RabbitMQTransportBindingElement _transport;
        
        /// <summary>
        /// Creates a new instance of the RabbitMQBinding class initialized
        /// to use the Protocols.DefaultProtocol. The broker must be set
        /// before use.
        /// </summary>
        public RabbitMQBinding()
            : this(Protocols.DefaultProtocol)
        { }
        
        /// <summary>
        /// Uses the specified protocol. The broker must be set before use.
        /// </summary>
        /// <param name="protocol">The protocol version to use</param>
        public RabbitMQBinding(IProtocol protocol)
        {
            BrokerProtocol = protocol;

            // Set defaults
            OneWayOnly = true;
            ExactlyOnce = false;

            Name = "RabbitMQBinding";
            Namespace = "http://schemas.rabbitmq.com/2007/RabbitMQ/";

            Initialize();
        }

        public override BindingElementCollection CreateBindingElements()
        {
            _transport.BrokerProtocol = BrokerProtocol;
            _transport.TransactedReceiveEnabled = ExactlyOnce;
            _transport.TTL = TTL;
            _transport.PersistentDelivery = PersistentDelivery;
            _transport.AutoBindExchange = AutoBindExchange;
            _transport.ReplyToQueue = ReplyToQueue;
            _transport.ReplyToExchange = ReplyToExchange;
            _transport.OneWayOnly = OneWayOnly;
            _transport.ApplicationId = ApplicationId;
            _transport.MessageFormat = MessageFormat;
            _transport.HeaderNamespace = HeaderNamespace;
            _transport.Immediate = Immediate;
            _transport.Mandatory = Mandatory; 

            if (ReaderQuotas != null)
            {
                ReaderQuotas.CopyTo(_textEncoding.ReaderQuotas);
                ReaderQuotas.CopyTo(_mtomEncoding.ReaderQuotas);
                ReaderQuotas.CopyTo(_binaryEncoding.ReaderQuotas);
            }
            
            BindingElementCollection elements = new BindingElementCollection();

            if (!OneWayOnly)
            {
                elements.Add(_duplex);
            }

            elements.Add(_binaryEncoding);
            elements.Add(_mtomEncoding);
            elements.Add(_textEncoding);
            elements.Add(_transport);

            return elements;
        }

        private void Initialize()
        {
            lock (this)
            {
                if (!_isInitialized)
                {
                    _transport = new RabbitMQTransportBindingElement();
                    _textEncoding = new TextMessageEncodingBindingElement();
                    _mtomEncoding = new MtomMessageEncodingBindingElement();
                    _binaryEncoding = new BinaryMessageEncodingBindingElement();
                        
                    _duplex = new CompositeDuplexBindingElement();
                    
                    _isInitialized = true;
                }
            }
        }

        /// <summary>
        /// Gets the scheme used by the binding
        /// </summary>
        public override string Scheme
        {
            get { return CurrentVersion.Scheme; }
        }

        /// <summary>
        /// Specifies the version of the AMQP protocol that should be used to communicate with the broker
        /// </summary>
        public IProtocol BrokerProtocol { get; set; }

        /// <summary>
        /// Gets the AMQP transport binding element
        /// </summary>
        public RabbitMQTransportBindingElement Transport
        {
            get { return _transport; }
        }
        
        /// <summary>
        /// Enables transactional message delivery
        /// </summary>
        public bool ExactlyOnce { get; set; }

        /// <summary>
        /// Message time to live
        /// </summary>
        public string TTL { get; set; }

        /// <summary>
        /// ReplyTo queue name for duplex communication
        /// </summary>
        /// <remarks>If null will auto delete queue will be generated</remarks>
        public string ReplyToQueue { get; set; }

        /// <summary>
        /// ReplyTo exchange URI for duplex communication callbacks
        /// </summary>
        public Uri ReplyToExchange { get; set; }

        /// <summary>
        /// Exchange name to bind the listening queue. Value can be null.
        /// </summary>
        /// <remarks>If null queue will not be binded automaticaly</remarks>
        public string AutoBindExchange { get; set; }

        /// <summary>
        /// Defines messages delivery mode
        /// </summary>
        public bool PersistentDelivery { get; set; }

        /// <summary>
        /// Defines if one way or duplex comunication is required over this binding
        /// </summary>
        public bool OneWayOnly { get; set; }

        /// <summary>
        /// Application identificator. If not blanked will attached to the published messages. 
        /// </summary>
        /// <remarks>
        /// If IgnoreSelfPublished is True messages with same application id will be dropped. 
        /// </remarks>
        /// <remarks>
        /// If not blanked application id will be used as queue name if queue name is not supplied by listener address or ReplyToQueue
        /// </remarks>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Defines which message format to use when messages are sent
        /// </summary>
        /// <remarks>
        /// Received messages may be in all supported format even for the same binding
        /// </remarks>
        public MessageFormat MessageFormat { get; set; }

        /// <summary>
        /// Specify SOAP headers namespace to map to AMQP message header 
        /// </summary>
        public string HeaderNamespace { get; set; }

        /// <summary>
        /// Serializer quotas
        /// </summary>
        public XmlDictionaryReaderQuotas ReaderQuotas { get; set; }

        /// <summary>
        /// This flag tells the server how to react if the message cannot be routed to a queue. If this flag is set, the server will return an unroutable message with a Return method. If this flag is zero, the server silently drops the message.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// This flag tells the server how to react if the message cannot be routed to a queue consumer immediately. If this flag is set, the server will return an undeliverable message with a Return method. If this flag is zero, the server will queue the message, but with no guarantee that it will ever be consumed.
        /// </summary>
        public bool Immediate { get; set; }
    }

}
