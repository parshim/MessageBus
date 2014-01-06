using System;
using System.Threading;
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
            BasicGetResult result;
            DateTime startTime = DateTime.Now;

            do
            {
                result = _model.BasicGet(_queue, true);

                TimeSpan elapsedTime = DateTime.Now - startTime;
                TimeSpan remainingTime = timeout.Subtract(elapsedTime);
                if (remainingTime <= TimeSpan.Zero)
                {
                    return result;
                }

                if (result == null)
                {
                    Thread.Sleep(0);
                }

            } while (result == null);

            return result;
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