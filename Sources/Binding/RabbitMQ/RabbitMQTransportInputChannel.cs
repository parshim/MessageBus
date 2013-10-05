using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections;
using System.Collections.Generic;

using MessageBus.Binding.RabbitMQ.Clent.Extensions;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_9;

namespace MessageBus.Binding.RabbitMQ
{
    internal sealed class RabbitMQTransportInputChannel : RabbitMQInputChannelBase
    {
        private readonly string _bindToExchange;
        private readonly RabbitMQTransportBindingElement _bindingElement;
        private readonly Dictionary<string, MessageEncoder> _encoders = new Dictionary<string, MessageEncoder>();
        
        private IModel _model;
        private IMessageQueue _messageQueue;
        
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
            
            _messageQueue = null;
        }

        public override Message Receive(TimeSpan timeout)
        {
            try
            {
                BasicDeliverEventArgs result;

                if (!_messageQueue.Dequeue(timeout, out result))
                {
                    return null;
                }
#if VERBOSE
                DebugHelper.Start();
#endif
                string contentType = result.BasicProperties.ContentType;

                if (!_encoders.ContainsKey(contentType))
                {
                    _messageQueue.DropMessage(result.DeliveryTag);

                    return null;
                }

                MessageEncoder encoder = _encoders[contentType];

                _messageQueue.AcceptMessage(result.DeliveryTag);

                Message message = encoder.ReadMessage(new MemoryStream(result.Body), int.MaxValue);
                message.Headers.To = LocalAddress.Uri;
#if VERBOSE
                DebugHelper.Stop(" #### Message.Receive {{\n\tAction={2}, \n\tBytes={1}, \n\tTime={0}ms}}.",
                        msg.Body.Length,
                        result.Headers.Action.Remove(0, result.Headers.Action.LastIndexOf('/')));
#endif
                return message;
            }
            catch (EndOfStreamException)
            {
                if (_messageQueue == null || _messageQueue.ShutdownReason != null && _messageQueue.ShutdownReason.ReplyCode != Constants.ReplySuccess)
                {
                    OnFaulted();
                }
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
            return _messageQueue.WaitForMessage(timeout);
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
                _model.BasicCancel(_messageQueue.ConsumerTag);

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

            string queue = uri.Endpoint;

            IDictionary args = new Dictionary<String, Object>();

            int ttl;
            if (!string.IsNullOrEmpty(_bindingElement.TTL) && int.TryParse(_bindingElement.TTL, out ttl))
            {
                args.Add("x-message-ttl", ttl);
            }

            //Create a queue for messages destined to this service, bind it to the service URI routing key
            bool autoDelete = false;

            if (queue == null)
            {
                queue = Guid.NewGuid().ToString();
                
                autoDelete = true;
            }

            queue = _model.QueueDeclare(queue, true, autoDelete, autoDelete, args);

            if (!string.IsNullOrEmpty(_bindToExchange))
            {
                _model.QueueBind(queue, _bindToExchange, uri.RoutingKey);
            }

            QueueingBasicConsumerBase queueingBasicConsumer;

            // Create queue
            if (_bindingElement.TransactedReceiveEnabled)
            {
                queueingBasicConsumer = new TransactionalQueueConsumer(_model);
            }
            else
            {
                queueingBasicConsumer = new QueueingNoAckBasicConsumer(_model);
            }

            _messageQueue = queueingBasicConsumer;

            //Listen to the queue
            bool noAck = !_bindingElement.TransactedReceiveEnabled;

            _model.BasicConsume(queue, noAck, queueingBasicConsumer);

#if VERBOSE
            DebugHelper.Stop(" ## In.Channel.Open {{\n\tAddress={1}, \n\tTime={0}ms}}.", LocalAddress.Uri.PathAndQuery);
#endif
            OnOpened();
        }

    }
}
