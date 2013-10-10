using System;
using System.Transactions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    /// <summary>
    /// Queue consumer with transaction support
    /// </summary>
    public class TransactionalQueueConsumer : QueueingBasicConsumerBase
    {
        public TransactionalQueueConsumer()
        {
        }

        public TransactionalQueueConsumer(IModel model) : base(model)
        {
        }

        public TransactionalQueueConsumer(IModel model, SharedQueue<BasicDeliverEventArgs> queue)
            : base(model, queue)
        {
        }
        
        public override void DropMessage(ulong deliveryTag)
        {
            Model.BasicAck(deliveryTag, false);
        }

        public override void AcceptMessage(ulong deliveryTag)
        {
            if (Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(new ConsumptionEnslistment(deliveryTag, Model), EnlistmentOptions.None);
            }
        }
    }
}