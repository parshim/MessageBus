using System;
using System.Threading;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    public class NoAckMessageReceiver : IMessageReceiver
    {
        private readonly IModel _model;
        private readonly string _queue;

        private const int MinimalThreadSleepTickTime = 1;

        public NoAckMessageReceiver(IModel model, string queue)
        {
            _model = model;
            _queue = queue;
        }

        public BasicGetResult Receive(TimeSpan timeout)
        {
            DateTime startTime = DateTime.Now;
            TimeSpan remainingTime;

            do
            {
                BasicGetResult result = _model.BasicGet(_queue, true);

                if (result != null)
                {
                    return result;
                }

                Thread.Sleep(MinimalThreadSleepTickTime);

                TimeSpan elapsedTime = DateTime.Now - startTime;
                remainingTime = timeout.Subtract(elapsedTime);

            } while (remainingTime > TimeSpan.Zero);

            return null;
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