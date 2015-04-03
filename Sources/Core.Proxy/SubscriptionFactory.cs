using System;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public class SubscriptionFactory<T> : ISubscriptionFactory<T> where T : class
    {
        private readonly IBus _bus;
        private readonly Action<ISubscriberConfigurator> _configurator;
        private readonly IMessageFactory _messageFactory;

        public SubscriptionFactory(IBus bus, Action<ISubscriberConfigurator> configurator = null)
        {
            _bus = bus;
            _configurator = configurator;

            string ns = typeof(T).GetMessageNamespace();

            _messageFactory = new MessageFactory(ns);
        }
        
        public ISubscriptionSelector<T> Subscribe()
        {
            ISubscriber subscriber = _bus.CreateSubscriber(_configurator);

            subscriber.Open();

            return new SubscriptionSelector<T>(subscriber, _messageFactory);
        }
    }
}