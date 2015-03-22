using System;
using System.Configuration;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    public sealed class RabbitMQTransportElement : TransportElement
    {
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            if (bindingElement == null)
                throw new ArgumentNullException("bindingElement");

            RabbitMQTransportBindingElement rabbind = bindingElement as RabbitMQTransportBindingElement;
            if (rabbind == null)
            {
                throw new ArgumentException(
                    string.Format("Invalid type for binding. Expected {0}, Passed: {1}",
                        typeof(RabbitMQBinding).AssemblyQualifiedName,
                        bindingElement.GetType().AssemblyQualifiedName));
            }

            rabbind.PersistentDelivery = PersistentDelivery;
            rabbind.AutoBindExchange = AutoBindExchange;
            rabbind.TTL = TTL;
            rabbind.AutoDelete = AutoDelete;
            rabbind.BrokerProtocol = Protocol;
            rabbind.TransactedReceiveEnabled = ExactlyOnce;
            rabbind.ReplyToQueue = ReplyToQueue;
            rabbind.ReplyToExchange = ReplyToExchange != null ? new Uri(ReplyToExchange) : null;
            rabbind.OneWayOnly = OneWayOnly;
            rabbind.MessageFormat = MessageFormat;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            RabbitMQTransportElement element = from as RabbitMQTransportElement;
            if (element != null)
            {
                PersistentDelivery = element.PersistentDelivery;
                AutoBindExchange = element.AutoBindExchange;
                TTL = element.TTL;
                ProtocolVersion = element.ProtocolVersion;
                ExactlyOnce = element.ExactlyOnce;
                ReplyToQueue = element.ReplyToQueue;
                ReplyToExchange = element.ReplyToExchange;
                OneWayOnly = element.OneWayOnly;
                MessageFormat = element.MessageFormat;
                ApplicationId = element.ApplicationId;
                HeaderNamespace = element.HeaderNamespace;
            }
        }

        protected override BindingElement CreateBindingElement()
        {
            TransportBindingElement element = CreateDefaultBindingElement();
            
            ApplyConfiguration(element);
            
            return element;
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new RabbitMQTransportBindingElement();
        }

        protected override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);

            if (bindingElement == null)
                throw new ArgumentNullException("bindingElement");

            RabbitMQTransportBindingElement rabbind = bindingElement as RabbitMQTransportBindingElement;
            if (rabbind == null)
            {
                throw new ArgumentException(
                    string.Format("Invalid type for binding. Expected {0}, Passed: {1}",
                        typeof(RabbitMQBinding).AssemblyQualifiedName,
                        bindingElement.GetType().AssemblyQualifiedName));
            }

            PersistentDelivery = rabbind.PersistentDelivery;
            AutoBindExchange = rabbind.AutoBindExchange;
            TTL = rabbind.TTL;
            ProtocolVersion = rabbind.BrokerProtocol.ApiName;
            ReplyToQueue = rabbind.ReplyToQueue;
            ReplyToExchange = rabbind.ReplyToExchange.ToString();
            OneWayOnly = rabbind.OneWayOnly;
            MessageFormat = rabbind.MessageFormat;
            ApplicationId = rabbind.ApplicationId;
            HeaderNamespace = rabbind.HeaderNamespace;
        }

        public override Type BindingElementType
        {
            get { return typeof(RabbitMQTransportElement); }
        }

        [ConfigurationProperty("autoBindExchange", IsRequired = true, DefaultValue = "")]
        public string AutoBindExchange
        {
            get { return ((string)base["autoBindExchange"]); }
            set { base["autoBindExchange"] = value; }
        }

        [ConfigurationProperty("persistentDelivery", IsRequired = false, DefaultValue = false)]
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
        /// Specifies the port of the broker that the binding should connect to.
        /// </summary>
        [ConfigurationProperty("TTL", IsRequired = false, DefaultValue = "")]
        public string TTL
        {
            get { return ((string)base["TTL"]); }
            set { base["TTL"] = value; }
        }

        /// <summary>
        /// Specifies if the queue is temporary.
        /// </summary>
        [ConfigurationProperty("AutoDelete", IsRequired = false, DefaultValue = false)]
        public bool AutoDelete
        {
            get { return ((bool)base["AutoDelete"]); }
            set { base["AutoDelete"] = value; }
        }

        /// <summary>
        /// Enables transactional message delivery
        /// </summary>
        [ConfigurationProperty("exactlyOnce", IsRequired = false, DefaultValue = false)]
        public bool ExactlyOnce
        {
            get { return ((bool)base["exactlyOnce"]); }
            set { base["exactlyOnce"] = value; }
        }
        
        /// <summary>
        /// Specifies the protocol version to use when communicating with the broker
        /// </summary>
        [ConfigurationProperty("protocolversion", DefaultValue = "DefaultProtocol")]
        public string ProtocolVersion
        {
            get
            {
                return ((string)base["protocolversion"]);
            }
            set
            {
                base["protocolversion"] = value;
                GetProtocol();
            }
        }

        /// <summary>
        /// ReplyTo exchange URI for duplex communication callbacks
        /// </summary>
        [ConfigurationProperty("replyToExchange", DefaultValue = "")]
        public string ReplyToExchange
        {
            get
            {
                return ((string)base["replyToExchange"]);
            }
            set
            {
                base["replyToExchange"] = value;
            }
        }

        /// <summary>
        /// ReplyTo queue name for duplex communication
        /// </summary>
        /// <remarks>If null will auto delete queue will be generated</remarks>
        [ConfigurationProperty("replyToQueue", DefaultValue = "")]
        public string ReplyToQueue
        {
            get
            {
                return ((string)base["replyToQueue"]);
            }
            set
            {
                base["replyToQueue"] = value;
            }
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

        private IProtocol GetProtocol()
        {
            IProtocol result = Protocols.DefaultProtocol;

            if (result == null)
            {
                throw new ConfigurationErrorsException(string.Format("'{0}' is not a valid AMQP protocol name", ProtocolVersion));
            }

            return result;
        }

        /// <summary>
        /// Gets the protocol version specified by the current configuration
        /// </summary>
        public IProtocol Protocol { get { return GetProtocol(); } }
        
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
