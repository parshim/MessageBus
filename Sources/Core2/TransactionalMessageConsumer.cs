using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class TransactionalMessageConsumer : MessageConsumer
    {
        public TransactionalMessageConsumer(string busId, IModel model, IMessageHelper messageHelper, ISendHelper sendHelper, Dictionary<string, ISerializer> serializers, IErrorSubscriber errorSubscriber, TaskScheduler scheduler, bool receiveSelfPublish)
            : base(busId, model, messageHelper, sendHelper, serializers, errorSubscriber, scheduler, receiveSelfPublish)
        {
        }

        protected override RawBusMessage HandleMessage(ICallHandler handler, RawBusMessage message, bool redelivered, ulong deliveryTag)
        {
            try
            {
                RawBusMessage replyMessage = base.HandleMessage(handler, message, redelivered, deliveryTag);

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
            catch (Exception)
            {
                Model.BasicNack(deliveryTag, false, true);

                throw;
            }
        }
    }
}