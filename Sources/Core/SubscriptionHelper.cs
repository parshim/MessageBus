using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class SubscriptionHelper
    {
        private readonly IBus _bus;

        public SubscriptionHelper(IBus bus)
        {
            _bus = bus;
        }

        public ISubscriber RegisterSubscribtion(Type subscribtion, Action<ISubscriberConfigurator> configure = null)
        {
            ISubscriber subscriber = _bus.CreateSubscriber(configure);

            throw new NotImplementedException();

            return subscriber;
        }
    }
}
