using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    /// <summary>
    /// Basic queue consumer implementation, message acknowledge is user responsibility 
    /// </summary>
    public class QueueingNoAckBasicConsumer : QueueingBasicConsumerBase
    {
        public QueueingNoAckBasicConsumer()
        {
        }

        public QueueingNoAckBasicConsumer(IModel model) : base(model)
        {
        }

        public QueueingNoAckBasicConsumer(IModel model, SharedQueue<BasicDeliverEventArgs> queue)
            : base(model, queue)
        {
        }
    }
    
}
