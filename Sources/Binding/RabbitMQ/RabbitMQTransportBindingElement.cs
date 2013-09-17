using System;
using System.ServiceModel.Description;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    using System.ServiceModel.Channels;

    /// <summary>
    /// Represents the binding element used to specify AMQP transport for transmitting messages.
    /// </summary>
    public sealed class RabbitMQTransportBindingElement : TransportBindingElement, ITransactedBindingElement
    {
        /// <summary>
        /// Creates a new instance of the RabbitMQTransportBindingElement Class using the default protocol.
        /// </summary>
        public RabbitMQTransportBindingElement()
        {
            MaxReceivedMessageSize = RabbitMQBinding.DefaultMaxMessageSize;
        }

        private RabbitMQTransportBindingElement(RabbitMQTransportBindingElement other)
        {
            BrokerProtocol = other.BrokerProtocol;
            MaxReceivedMessageSize = other.MaxReceivedMessageSize;
            TransactedReceiveEnabled = other.TransactedReceiveEnabled;
            TTL = other.TTL;
            PersistentDelivery = other.PersistentDelivery;
            AutoBindExchange = other.AutoBindExchange;
            ReplyToQueue = other.ReplyToQueue;
            ReplyToExchange = other.ReplyToExchange;
            OneWayOnly = other.OneWayOnly;
        }
        
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return new RabbitMQTransportChannelFactory<TChannel>(context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            bool autoDelete = false;

            string bindToExchange = AutoBindExchange;

            if (context.ListenUriBaseAddress == null)
            {
                if (ReplyToExchange == null)
                {
                    return null;
                }

                RabbitMQUri uri = new RabbitMQUri(ReplyToExchange);

                context.ListenUriMode = ListenUriMode.Explicit;
                
                if (ReplyToQueue == null)
                {
                    autoDelete = true;

                    bindToExchange = uri.Endpoint;

                    context.ListenUriRelativeAddress = Guid.NewGuid().ToString();
                }
                else
                {
                    context.ListenUriRelativeAddress = ReplyToQueue;
                }

                if (uri.Port.HasValue)
                {
                    context.ListenUriBaseAddress = new Uri(string.Format("{0}://{1}:{2}/", Scheme, uri.Host, uri.Port));
                }
                else
                {
                    context.ListenUriBaseAddress = new Uri(string.Format("{0}://{1}/", Scheme, uri.Host));
                }
            }

            Uri listenUri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress ?? "");
            
            return new RabbitMQTransportChannelListener<TChannel>(context, listenUri, autoDelete, bindToExchange);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IOutputChannel);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context.ListenUriMode == ListenUriMode.Unique && ReplyToExchange == null)
            {
                return false;
            }

            return typeof(TChannel) == typeof(IInputChannel);
        }

        public override BindingElement Clone()
        {
            return new RabbitMQTransportBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }

        /// <summary>
        /// Gets the scheme used by the binding
        /// </summary>
        public override string Scheme
        {
            get { return CurrentVersion.Scheme; }
        }
        
        /// <summary>
        /// Enables transactional message delivery
        /// </summary>
        public bool TransactedReceiveEnabled
        {
            get; set;
        }

        /// <summary>
        /// Enables transactional message delivery
        /// </summary>
        public string TTL
        {
            get; set;
        }

        /// <summary>
        /// The largest receivable encoded message
        /// </summary>
        public override long MaxReceivedMessageSize
        {
            get; set;
        }
        
        /// <summary>
        /// Specifies the version of the AMQP protocol that should be used to 
        /// communicate with the broker
        /// </summary>
        public IProtocol BrokerProtocol
        {
            get; set;
        }

        /// <summary>
        /// Exchange name to bind the listening queue. Value can be null.
        /// </summary>
        /// <remarks>If null queue will not be binded automaticaly</remarks>
        public string AutoBindExchange
        {
            get; set;
        }

        /// <summary>
        /// Defines messages delivery mode
        /// </summary>
        public bool PersistentDelivery
        {
            get; set;
        }

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
        /// Defines if one way or duplex comunication is required over this binding
        /// </summary>
        public bool OneWayOnly { get; set; }
    }
}
