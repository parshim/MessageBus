using System.Transactions;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    public class ConsumptionEnslistment : IEnlistmentNotification
    {
        private readonly ulong _deliveryTag;
        private readonly IModel _model;

        public ConsumptionEnslistment(ulong deliveryTag, IModel model)
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