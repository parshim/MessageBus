using System;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    public class NoAckMessageReceiver : IMessageReceiver
    {
        private readonly IModel _model;
        private readonly string _queue;

        public NoAckMessageReceiver(IModel model, string queue)
        {
            _model = model;
            _queue = queue;
        }

        public BasicGetResult Receive(TimeSpan timeout)
        {
            return _model.BasicGet(_queue, true);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return true;
        }

        public void DropMessage(ulong deliveryTag)
        {
            
        }

        public void AcceptMessage(ulong deliveryTag)
        {
            
        }
    }
}