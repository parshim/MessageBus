using System.Transactions;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    internal class TransactionalDispatchingEnslistment : IEnlistmentNotification
    {
        private readonly IModel _model;

        public TransactionalDispatchingEnslistment(IModel model)
        {
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
                _model.TxCommit();
            }

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            if (_model.IsOpen)
            {
                _model.TxRollback();
            }

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}