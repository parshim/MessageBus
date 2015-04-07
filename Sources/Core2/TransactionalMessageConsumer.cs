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

        protected override Task StartConsumerTask(ulong deliveryTag, IBasicProperties properties, byte[] body)
        {
            Task task = base.StartConsumerTask(deliveryTag, properties, body);

            return task.ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    Model.BasicAck(deliveryTag, false);
                }
                else
                {
                    Model.BasicNack(deliveryTag, false, true);
                }
            });
        }
    }
}