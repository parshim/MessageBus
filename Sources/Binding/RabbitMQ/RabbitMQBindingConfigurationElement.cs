using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Reflection;
using System.Xml;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{  
    /// <summary>
    /// Represents the configuration for a RabbitMQBinding.
    /// </summary>
    /// <remarks>
    /// This configuration element should be imported into the client
    /// and server configuration files to provide declarative configuration 
    /// of a AMQP bound service.
    /// </remarks>
    public sealed class RabbitMQBindingConfigurationElement : StandardBindingElement
    {
        /// <summary>
        /// Creates a new instance of the RabbitMQBindingConfigurationElement
        /// Class initialized with values from the specified configuration.
        /// </summary>
        /// <param name="configurationName"></param>
        public RabbitMQBindingConfigurationElement(string configurationName)
            : base(configurationName) {
        }
     
        /// <summary>
        /// Creates a new instance of the RabbitMQBindingConfigurationElement Class.
        /// </summary>
        public RabbitMQBindingConfigurationElement()
            : this(null) {
        }


        protected override void InitializeFrom(System.ServiceModel.Channels.Binding binding)
        {
            base.InitializeFrom(binding);
            RabbitMQBinding rabbind = binding as RabbitMQBinding;
            if (rabbind != null)
            {
                ExactlyOnce = rabbind.ExactlyOnce;
                TTL = rabbind.TTL;
                ReplyToExchange = rabbind.ReplyToQueue;
                AutoBindExchange = rabbind.AutoBindExchange;
                PersistentDelivery = rabbind.PersistentDelivery;
                OneWayOnly = rabbind.OneWayOnly;
                ReplyToQueue = rabbind.ReplyToQueue;
                ApplicationId = rabbind.ApplicationId;
                HeaderNamespace = rabbind.HeaderNamespace;
                MessageFormat = rabbind.MessageFormat;
                Immediate = rabbind.Immediate;
                Mandatory = rabbind.Mandatory;
                ReadQuotas(rabbind.ReaderQuotas);
            }
        }

        protected override void OnApplyConfiguration(System.ServiceModel.Channels.Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException("binding");

            RabbitMQBinding rabbind = binding as RabbitMQBinding;
            if (rabbind == null)
            {
                throw new ArgumentException(
                    string.Format("Invalid type for binding. Expected {0}, Passed: {1}", 
                        typeof(RabbitMQBinding).AssemblyQualifiedName, 
                        binding.GetType().AssemblyQualifiedName));
            }

            rabbind.AutoBindExchange = AutoBindExchange;
            rabbind.BrokerProtocol = Protocol;
            rabbind.ExactlyOnce = ExactlyOnce;
            rabbind.OneWayOnly = OneWayOnly;
            rabbind.PersistentDelivery = PersistentDelivery;
            rabbind.ReplyToExchange = ReplyToExchange == null ? null : new Uri(ReplyToExchange);
            rabbind.ReplyToQueue = ReplyToQueue;
            rabbind.TTL = TTL;
            rabbind.ApplicationId = ApplicationId;
            rabbind.MessageFormat = MessageFormat;
            rabbind.HeaderNamespace = HeaderNamespace;
            rabbind.Immediate = Immediate;
            rabbind.Mandatory = Mandatory;

            ApplyQuotas(rabbind.ReaderQuotas);
        }

        private void ApplyQuotas(XmlDictionaryReaderQuotas target)
        {
            if (ReaderQuotas.MaxDepth != 0)
                target.MaxDepth = ReaderQuotas.MaxDepth;
            if (ReaderQuotas.MaxStringContentLength != 0)
                target.MaxStringContentLength = ReaderQuotas.MaxStringContentLength;
            if (ReaderQuotas.MaxArrayLength != 0)
                target.MaxArrayLength = ReaderQuotas.MaxArrayLength;
            if (ReaderQuotas.MaxBytesPerRead != 0)
                target.MaxBytesPerRead = ReaderQuotas.MaxBytesPerRead;
            if (ReaderQuotas.MaxNameTableCharCount != 0)
                target.MaxNameTableCharCount = ReaderQuotas.MaxNameTableCharCount;
        }
        
        private void ReadQuotas(XmlDictionaryReaderQuotas source)
        {
            if (source.MaxDepth != 0)
                ReaderQuotas.MaxDepth = source.MaxDepth;
            if (source.MaxStringContentLength != 0)
                ReaderQuotas.MaxStringContentLength = source.MaxStringContentLength;
            if (source.MaxArrayLength != 0)
                ReaderQuotas.MaxArrayLength = source.MaxArrayLength;
            if (source.MaxBytesPerRead != 0)
                ReaderQuotas.MaxBytesPerRead = source.MaxBytesPerRead;
            if (source.MaxNameTableCharCount != 0)
                ReaderQuotas.MaxNameTableCharCount = source.MaxNameTableCharCount;
        }

        /// <summary>
        /// Enables transactional message delivery
        /// </summary>
        [ConfigurationProperty("exactlyOnce", DefaultValue = false)]
        public bool ExactlyOnce
        {
            get { return ((bool)base["exactlyOnce"]); }
            set { base["exactlyOnce"] = value; }
        }
        
        /// <summary>
        /// Defines messages delivery mode
        /// </summary>
        [ConfigurationProperty("persistentDelivery", DefaultValue = false)]
        public bool PersistentDelivery
        {
            get { return ((bool)base["persistentDelivery"]); }
            set { base["persistentDelivery"] = value; }
        }

        /// <summary>
        /// Defines if one way or duplex comunication is required over this binding
        /// </summary>
        [ConfigurationProperty("oneWayOnly", DefaultValue = true)]
        public bool OneWayOnly
        {
            get { return ((bool)base["oneWayOnly"]); }
            set { base["oneWayOnly"] = value; }
        }


        /// <summary>
        /// This flag tells the server how to react if the message cannot be routed to a queue. If this flag is set, the server will return an unroutable message with a Return method. If this flag is zero, the server silently drops the message.
        /// </summary>
        [ConfigurationProperty("mandatory", DefaultValue = false)]
        public bool Mandatory
        {
            get { return ((bool)base["mandatory"]); }
            set { base["mandatory"] = value; }
        }

        /// <summary>
        /// This flag tells the server how to react if the message cannot be routed to a queue consumer immediately. If this flag is set, the server will return an undeliverable message with a Return method. If this flag is zero, the server will queue the message, but with no guarantee that it will ever be consumed.
        /// </summary>
        [ConfigurationProperty("immediate", DefaultValue = false)]
        public bool Immediate
        {
            get { return ((bool)base["immediate"]); }
            set { base["immediate"] = value; }
        }
        
        /// <summary>
        /// Application identificator. If not blanked will attached to the published messages. 
        /// </summary>
        /// <remarks>
        /// If IgnoreSelfPublished is True messages with same application id will be dropped. 
        /// </remarks>
        /// <remarks>
        /// If not blanked application id will be used as queue name if queue name is not supplied by listener address or ReplyToQueue
        /// </remarks>
        [ConfigurationProperty("applicationId", DefaultValue = null)]
        public string ApplicationId
        {
            get { return ((string)base["applicationId"]); }
            set { base["applicationId"] = value; }
        }

        /// <summary>
        /// Specify SOAP headers namespace to map to AMQP message header 
        /// </summary>
        [ConfigurationProperty("headerNamespace", DefaultValue = null)]
        public string HeaderNamespace
        {
            get { return ((string)base["headerNamespace"]); }
            set { base["headerNamespace"] = value; }
        }

        /// <summary>
        /// Defines which message format to use when messages are sent
        /// </summary>
        /// <remarks>
        /// Received messages may be in all supported format even for the same binding
        /// </remarks>
        [ConfigurationProperty("messageFormat", DefaultValue = MessageFormat.Text)]
        public MessageFormat MessageFormat
        {
            get { return ((MessageFormat)base["messageFormat"]); }
            set { base["messageFormat"] = value; }
        }

        /// <summary>
        /// ReplyTo exchange URI for duplex communication callbacks
        /// </summary>
        [ConfigurationProperty("replyToExchange", DefaultValue = null)]
        public string ReplyToExchange
        {
            get { return ((string)base["replyToExchange"]); }
            set { base["replyToExchange"] = value; }
        }

        /// <summary>
        /// ReplyTo queue name for duplex communication
        /// </summary>
        /// <remarks>If null will auto delete queue will be generated</remarks>
        [ConfigurationProperty("replyToQueue", DefaultValue = null)]
        public string ReplyToQueue
        {
            get { return ((string)base["replyToQueue"]); }
            set { base["replyToQueue"] = value; }
        }

        /// <summary>
        /// Exchange name to bind the listening queue. Value can be null.
        /// </summary>
        /// <remarks>If null queue will not be binded automaticaly</remarks>
        [ConfigurationProperty("autoBindExchange", DefaultValue = null)]
        public string AutoBindExchange
        {
            get { return ((string)base["autoBindExchange"]); }
            set { base["autoBindExchange"] = value; }
        }
        
        /// <summary>
        /// Specifies message TTL. For client side binding it will be per message TTL, for service side binding it will be per-queue message TTL. Use null or discard to diable message TTL.
        /// </summary>
        [ConfigurationProperty("TTL", DefaultValue = null)]
        public string TTL
        {
            get { return ((string)base["TTL"]); }
            set { base["TTL"] = value; }
        }
        
        /// <summary>
        /// Specifies the protocol version to use when communicating with the broker
        /// </summary>
        [ConfigurationProperty("protocolversion", DefaultValue = "DefaultProtocol")]
        public string ProtocolVersion
        {
            get {
                return ((string)base["protocolversion"]);
            }
            set {
                base["protocolversion"] = value;
                GetProtocol();
            }
        }

        /// <summary>
        /// Gets or sets constraints on the complexity of SOAP messages that can be processed by endpoints configured with this binding.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Xml.XmlDictionaryReaderQuotas"/> that specifies the complexity constraints.
        /// </returns>
        [ConfigurationProperty("readerQuotas")]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get
            {
                return ((XmlDictionaryReaderQuotasElement)base["readerQuotas"]);
            }
            set
            {
                base["readerQuotas"] = value;
            }
        }

        private IProtocol GetProtocol() 
        {
            IProtocol result = Protocols.Lookup(ProtocolVersion);

            if (result == null) 
            {
                throw new ConfigurationErrorsException(string.Format("'{0}' is not a valid AMQP protocol name", ProtocolVersion));
            }

            return result;
        }

        /// <summary>
        /// Gets the protocol version specified by the current configuration
        /// </summary>
        public IProtocol Protocol
        {
            get 
            {
                return GetProtocol();
            }
        }

        protected override Type BindingElementType
        {
            get { return typeof(RabbitMQBinding); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection configProperties = base.Properties;
                foreach (PropertyInfo prop in GetType().GetProperties(BindingFlags.DeclaredOnly
                                                                           | BindingFlags.Public
                                                                           | BindingFlags.Instance))
                {
                    foreach (ConfigurationPropertyAttribute attr in prop.GetCustomAttributes(typeof(ConfigurationPropertyAttribute), false))
                    {
                        configProperties.Add(
                            new ConfigurationProperty(attr.Name, prop.PropertyType, attr.DefaultValue));
                    }
                }

                return configProperties;
            }
        }
    }
}
