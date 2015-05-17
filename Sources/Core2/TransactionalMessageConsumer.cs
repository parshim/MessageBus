using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class TransactionalMessageConsumer : MessageConsumer
    {
        public TransactionalMessageConsumer(IModel model, string busId, IMessageHelper messageHelper, Dictionary<string, ISerializer> serializers, IErrorSubscriber errorSubscriber, TaskScheduler scheduler, bool receiveSelfPublish) : base(model, busId, messageHelper, serializers, errorSubscriber, scheduler, receiveSelfPublish)
        {
        }

        protected override void HandleMessage(ICallHandler handler, RawBusMessage message, bool redelivered, ulong deliveryTag)
        {
            try
            {
                base.HandleMessage(handler, message, redelivered, deliveryTag);
            }
            catch (RejectMessageException)
            {
                // If reject message exception is thrown -> reject message without requeue it. 
                // Message will be lost or transfered to dead letter exchange by broker
                Model.BasicReject(deliveryTag, false);

                return;
            }
            catch (Exception)
            {
                Model.BasicReject(deliveryTag, true);

                throw;
            }

            Model.BasicAck(deliveryTag, false);
        }
    }
}