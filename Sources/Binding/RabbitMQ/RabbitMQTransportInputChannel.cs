using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections;
using System.Collections.Generic;

using MessageBus.Binding.RabbitMQ.Clent.Extensions;

using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    internal sealed class RabbitMQTransportInputChannel : RabbitMQInputChannelBase
    {
        private readonly string _bindToExchange;
        private readonly RabbitMQTransportBindingElement _bindingElement;
        private readonly Dictionary<string, MessageEncoder> _encoders = new Dictionary<string, MessageEncoder>();
        
        private IModel _model;
        private IMessageReceiver _messageReceiver;
        private string _queueName;

        public RabbitMQTransportInputChannel(BindingContext context, EndpointAddress address, string bindToExchange) : base(context, address)
        {
            _bindToExchange = bindToExchange;
            _bindingElement = context.Binding.Elements.Find<RabbitMQTransportBindingElement>();
            
            IEnumerable<MessageEncodingBindingElement> encoderElemements = context.BindingParameters.FindAll<MessageEncodingBindingElement>();

            foreach (MessageEncodingBindingElement encoderElem in encoderElemements)
            {
                MessageEncoder encoder = encoderElem.CreateMessageEncoderFactory().Encoder;

                if (encoder.ContentType == null)
                {
                    continue;
                }

                _encoders.Add(encoder.ContentType, encoder);
            }
        }

        public IModel Model
        {
            get { return _model; }
        }

        public string QueueName
        {
            get { return _queueName; }
        }

        public override Message Receive(TimeSpan timeout)
        {
            try
            {
#if VERBOSE
                DebugHelper.Start();
#endif
                BasicGetResult result = _messageReceiver.Receive(timeout);

                if (result == null)
                {
                    return null;
                }

                string contentType = result.BasicProperties.ContentType;

                if (!_encoders.ContainsKey(contentType))
                {
                    _messageReceiver.DropMessage(result.DeliveryTag);

                    return null;
                }

                MessageEncoder encoder = _encoders[contentType];

                _messageReceiver.AcceptMessage(result.DeliveryTag);

#if VERBOSE
                DebugHelper.Stop(" #### Message.Receive {{\n\tBytes={1}, \n\tTime={0}ms}}.",
                        result.Body.Length);
#endif

#if VERBOSE
                DebugHelper.Start();
#endif
                Message message = encoder.ReadMessage(new MemoryStream(result.Body), int.MaxValue);
                message.Headers.To = LocalAddress.Uri;

#if VERBOSE
                DebugHelper.Stop(" #### Message.DeSerialize {{\n\tAction={2}, \n\tBytes={1}, \n\tTime={0}ms}}.",
                        result.Body.Length,
                        message.Headers.Action);
#endif
                return message;
            }
            catch (EndOfStreamException)
            {
                Close();
                return null;
            }
        }
        
        public override bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = Receive(timeout);
            return message != null;
        }

        public override bool WaitForMessage(TimeSpan timeout)
        {
            return true;
        }

        public override void Close(TimeSpan timeout)
        {
            if (State == CommunicationState.Closed || State == CommunicationState.Closing)
            {
                return; // Ignore the call, we're already closing.
            }

            OnClosing();
#if VERBOSE
            DebugHelper.Start();
#endif
            if (_model != null)
            {
                ConnectionManager.Instance.CloseModel(_model, timeout);
                
                _model = null;
            }

#if VERBOSE
            DebugHelper.Stop(" ## In.Channel.Close {{\n\tAddress={1}, \n\tTime={0}ms}}.", LocalAddress.Uri.PathAndQuery);
#endif
            OnClosed();
        }

        public override void Open(TimeSpan timeout)
        {
            if (State != CommunicationState.Created && State != CommunicationState.Closed)
                throw new InvalidOperationException(string.Format("Cannot open the channel from the {0} state.", State));

            OnOpening();
#if VERBOSE
            DebugHelper.Start();
#endif
            RabbitMQUri uri = new RabbitMQUri(LocalAddress.Uri);

            _model = ConnectionManager.Instance.OpenModel(uri, _bindingElement.BrokerProtocol, timeout);

            _queueName = uri.Endpoint;

            IDictionary<String, Object> args = new Dictionary<String, Object>();

            int ttl;
            if (!string.IsNullOrEmpty(_bindingElement.TTL) && int.TryParse(_bindingElement.TTL, out ttl))
            {
                args.Add("x-message-ttl", ttl);
            }

            //Create a queue for messages destined to this service, bind it to the service URI routing key
            bool autoDelete = string.IsNullOrEmpty(_queueName) || _bindingElement.AutoDelete;

            _queueName = _model.QueueDeclare(_queueName ?? "", true, autoDelete, autoDelete, args);

            if (!string.IsNullOrEmpty(_bindToExchange))
            {
                _model.QueueBind(_queueName, _bindToExchange, uri.RoutingKey);
            }

            // Create receiver
            if (_bindingElement.TransactedReceiveEnabled)
            {
                _messageReceiver = new TransactionalMessageReceiver(_model, _queueName);
            }
            else
            {
                _messageReceiver = new NoAckMessageReceiver(_model, _queueName);
            }
            
#if VERBOSE
            DebugHelper.Stop(" ## In.Channel.Open {{\n\tAddress={1}, \n\tTime={0}ms}}.", LocalAddress.Uri.PathAndQuery);
#endif
            OnOpened();
        }

    }
}
