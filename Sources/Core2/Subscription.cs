using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class Subscription : SubscriberBase
    {
        public Subscription(IModel model, string queue, IMessageConsumer consumer, object instance, SubscriberConfigurator configurator)
            : base(model, queue, consumer, configurator)
        {
            _helper.RegisterSubscription(instance);
        }
    }
}