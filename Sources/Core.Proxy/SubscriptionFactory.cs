using System;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public class SubscriptionFactory<T> : ISubscriptionFactory<T> where T : class
    {
        private readonly IBus _bus;
        private readonly IMessageFactory _messageFactory;

        public SubscriptionFactory(IBus bus)
        {
            _bus = bus;

            string ns = typeof(T).GetMessageNamespace();

            _messageFactory = new MessageFactory(ns);
        }

        public ISubscriptionSelector<T> Subscribe(Action<ISubscriberConfigurator> configurator)
        {
            ISubscriber subscriber = _bus.CreateSubscriber(configurator);

            subscriber.Open();

            return new SubscriptionSelector<T>(subscriber, _messageFactory);
        }
    }
}