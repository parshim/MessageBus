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
                Transaction.Current.EnlistVolatile(new TransactionalQueueConsumerEnslistment(deliveryTag, Model), EnlistmentOptions.None);
            }
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