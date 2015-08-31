using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public abstract class RpcPublisherBase : Publisher
    {
        protected readonly IRpcConsumer _consumer;

        protected RpcPublisherBase(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper, IRpcConsumer consumer)
            : base(model, busId, configuration, messageHelper, sendHelper)
        {
            _consumer = consumer;

            model.BasicConsume(Queue, true, consumer);
        }

        protected const string Queue = "amq.rabbitmq.reply-to";

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
                ReplyTo = Queue
            });
        }

        private static RawBusMessage CreateRawMessage<TData>(BusMessage<TData> busMessage)
        {
            RawBusMessage rawBusMessage = new RawBusMessage
            {
                Data = busMessage.Data
            };

            foreach (var header in busMessage.Headers)
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

        protected static string GenerateCorrelationId()
        {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return id.TrimEnd('=');
        }
    }
}