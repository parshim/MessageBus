using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Reflection;

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
                MaxMessageSize = rabbind.MaxMessageSize;
                ExactlyOnce = rabbind.ExactlyOnce;
                TTL = rabbind.TTL;
                ReplyToExchange = rabbind.ReplyToQueue;
                AutoBindExchange = rabbind.AutoBindExchange;
                PersistentDelivery = rabbind.PersistentDelivery;
                OneWayOnly = rabbind.OneWayOnly;
                ReplyToQueue = rabbind.ReplyToQueue;
                ApplicationId = rabbind.ApplicationId;
                IgnoreSelfPublished = rabbind.IgnoreSelfPublished;
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
            rabbind.Transport.MaxReceivedMessageSize = MaxMessageSize;
            rabbind.OneWayOnly = OneWayOnly;
            rabbind.PersistentDelivery = PersistentDelivery;
            rabbind.ReplyToExchange = ReplyToExchange == null ? null : new Uri(ReplyToExchange);
            rabbind.ReplyToQueue = ReplyToQueue;
            rabbind.TTL = TTL;
            rabbind.IgnoreSelfPublished = IgnoreSelfPublished;
            rabbind.ApplicationId = ApplicationId;
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
        /// Defines if messages published with same application id will be ignored
        /// </summary>
        [ConfigurationProperty("ignoreSelfPublished", DefaultValue = true)]
        public bool IgnoreSelfPublished
        {
            get { return ((bool)base["ignoreSelfPublished"]); }
            set { base["ignoreSelfPublished"] = value; }
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
        /// Specifies the maximum encoded message size
        /// </summary>
        [ConfigurationProperty("maxmessagesize", DefaultValue = 8192L)]
        public long MaxMessageSize
        {
            get { return (long)base["maxmessagesize"]; }
            set { base["maxmessagesize"] = value; }
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
