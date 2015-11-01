using System;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public abstract class RpcPublisherBase : Publisher
    {
        protected readonly IRpcConsumer _consumer;

        private readonly string _replyTo;

        protected RpcPublisherBase(IModel model, string busId, RpcPublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper, IRpcConsumer consumer)
            : base(model, busId, configuration, messageHelper, sendHelper)
        {
            _consumer = consumer;

            const string fastReplyQueue = "amq.rabbitmq.reply-to";

            if (configuration.UseFastReply)
            {
                model.BasicConsume(fastReplyQueue, true, consumer);

                _replyTo = fastReplyQueue;
            }
            else
            {
                QueueDeclareOk queueDeclare = model.QueueDeclare("", false, true, true, new Dictionary<string, object>());

                if (string.IsNullOrEmpty(configuration.ReplyExchange))
                {
                    // Use default exchange, no need to bind queue

                    _replyTo = queueDeclare.QueueName;
                }
                else
                {
                    // Bind queue to specified exchange by replyTo or generate unique routing key
                    string routingKey = configuration.ReplyTo ?? NewMiniGuid();
                    
                    model.QueueBind(queueDeclare.QueueName, configuration.ReplyExchange, routingKey);
                    
                    _replyTo = routingKey;
                }

                model.BasicConsume(queueDeclare.QueueName, true, consumer);
            }
        }
        
        protected override void OnMessageReturn(int replyCode, string replyText, RawBusMessage message)
        {
            _consumer.HandleBasicReturn(message.CorrelationId, replyCode, replyText);
        }

        protected void SendMessage<TData>(BusMessage<TData> busMessage, string id, bool persistant)
        {
            var rawBusMessage = CreateRawMessage(busMessage);

            _sendHelper.Send(new SendParams
            {
                BusId = _busId,
                Model = _model,
                BusMessage = rawBusMessage,
                CorrelationId = id,
                Serializer = _configuration.Serializer,
                Exchange = _configuration.Exchange,
                MandatoryDelivery = true,
                PersistentDelivery = persistant || _configuration.PersistentDelivery,
                RoutingKey = _configuration.RoutingKey,
                ReplyTo = _replyTo
            });
        }

        private RawBusMessage CreateRawMessage<TData>(BusMessage<TData> busMessage)
        {
            RawBusMessage rawBusMessage = new RawBusMessage
            {
                Data = busMessage.Data
            };

            foreach (var header in busMessage.Headers.Concat(_configuration.Headers))
            {
                rawBusMessage.Headers.Add(header);
            }

            return rawBusMessage;
        }

        protected static BusMessage<TReplyData> CreateBusMessage<TReplyData>(RawBusMessage replyMessage)
        {
            BusMessage<TReplyData> busReplyMessage = new BusMessage<TReplyData>
            {
                BusId = replyMessage.BusId,
                Sent = replyMessage.Sent,
                Data = (TReplyData)replyMessage.Data
            };

            foreach (var header in replyMessage.Headers)
            {
                busReplyMessage.Headers.Add(header);
            }

            return busReplyMessage;
        }

        protected static string NewMiniGuid()
        {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return id.TrimEnd('=');
        }
    }
}