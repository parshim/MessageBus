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

        public override BasicDeliverEventArgs Dequeue()
        {
            BasicDeliverEventArgs message = base.Dequeue();

            Transaction.Current.EnlistVolatile(new TransactionalQueueConsumerEnslistment(message.DeliveryTag, Model), EnlistmentOptions.None);
            
            return message;
        }

        public override bool Dequeue(TimeSpan timeout, out BasicDeliverEventArgs message)
        {
            bool dequeue = base.Dequeue(timeout, out message);

            if (dequeue && Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(new TransactionalQueueConsumerEnslistment(message.DeliveryTag, Model), EnlistmentOptions.None);
            }

            return dequeue;
        }

        public override BasicDeliverEventArgs DequeueNoWait()
        {
            BasicDeliverEventArgs message = base.DequeueNoWait();

            if (message != null && Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(new TransactionalQueueConsumerEnslistment(message.DeliveryTag, Model), EnlistmentOptions.None);
            }

            return message;
        }
    }

    public class TransactionalQueueConsumerEnslistment : IEnlistmentNotification
    {
        private readonly ulong _deliveryTag;
        private readonly IModel _model;

        public TransactionalQueueConsumerEnslistment(ulong deliveryTag, IModel model)
        {
            _deliveryTag = deliveryTag;
            _model = model;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            if (_model.IsOpen)
            {
                _model.BasicAck(_deliveryTag, false);
            }

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            if (_model.IsOpen)
            {
                _model.BasicNack(_deliveryTag, false, true);
            }

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}