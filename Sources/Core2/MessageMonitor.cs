using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class MessageMonitor : SubscriberBase
    {
        public MessageMonitor(IModel model, string queue, IBasicConsumer consumer, SubscriberConfigurator configurator) : base(model, queue, consumer, configurator)
        {
        }
    }

}