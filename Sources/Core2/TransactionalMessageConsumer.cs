using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class TransactionalMessageConsumer : MessageConsumer
    {
        private readonly IExceptionFilter _exceptionFilter;

        public TransactionalMessageConsumer(string busId, IModel model, IMessageHelper messageHelper, ISendHelper sendHelper, IExceptionFilter exceptionFilter, Dictionary<string, ISerializer> serializers, IErrorSubscriber errorSubscriber, bool receiveSelfPublish, bool neverReply, string replyExchange, ITrace trace)
            : base(busId, model, messageHelper, sendHelper, serializers, errorSubscriber, receiveSelfPublish, neverReply, replyExchange, trace)
        {
            _exceptionFilter = exceptionFilter;
        }

        protected override async Task<bool> ConsumeMessage(bool redelivered, ulong deliveryTag, IBasicProperties properties, byte[] body)
        {
            var processed = await base.ConsumeMessage(redelivered, deliveryTag, properties, body);

            if (!processed)
            {
                // Message can't be processed by consumer -> reject it
                Model.BasicNack(deliveryTag, false, false);
            }

            return processed;
        }

        protected override async Task<RawBusMessage> HandleMessage(ICallHandler handler, RawBusMessage message, bool redelivered, ulong deliveryTag)
        {
            try
            {
                RawBusMessage replyMessage = await base.HandleMessage(handler, message, redelivered, deliveryTag);

                Model.BasicAck(deliveryTag, false);

                return replyMessage;
            }
            catch (RejectMessageException)
            {
                // If reject message exception is thrown -> reject message without requeue it. 
                // Message will be lost or transfered to dead letter exchange by broker
                Model.BasicNack(deliveryTag, false, false);

                throw;
            }
            catch (Exception ex)
            {
                bool requeue = _exceptionFilter.Filter(ex, message, redelivered, deliveryTag);

                Model.BasicNack(deliveryTag, false, requeue);

                if (requeue)
                {
                    return null;
                }

                throw;
            }
        }
    }
}