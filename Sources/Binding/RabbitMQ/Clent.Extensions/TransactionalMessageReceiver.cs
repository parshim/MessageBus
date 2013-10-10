using System;
using System.Transactions;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    public class TransactionalMessageReceiver : IMessageReceiver
    {
        private readonly IModel _model;
        private readonly string _queue;

        public TransactionalMessageReceiver(IModel model, string queue)
        {
            _model = model;
            _queue = queue;
        }

        public BasicGetResult Receive(TimeSpan timeout)
        {
            return _model.BasicGet(_queue, false);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return true;
        }

        public void DropMessage(ulong deliveryTag)
        {
            _model.BasicAck(deliveryTag, false);
        }

        public void AcceptMessage(ulong deliveryTag)
        {
            if (Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(new ConsumptionEnslistment(deliveryTag, _model), EnlistmentOptions.None);
            }
        }
    }
}