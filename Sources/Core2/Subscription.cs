using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class Subscription : SubscriberBase
    {
        public Subscription(IModel model, string exchange, string queue, IMessageConsumer consumer, object instance, bool receiveSelfPublish)
            : base(model, exchange, queue, consumer, receiveSelfPublish)
        {
            _helper.RegisterSubscription(instance);
        }
    }
}