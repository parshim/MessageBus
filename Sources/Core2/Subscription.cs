using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class Subscription : SubscriberBase
    {
        public Subscription(IModel model, string queue, IMessageConsumer consumer, object instance, SubscriberConfigurator configurator, ISubscriptionHelper helper)
            : base(model, queue, consumer, configurator)
        {
            helper.RegisterSubscription(instance);
        }
    }
}