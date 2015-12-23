using System;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public interface ISubscriptionFactory<T> where T : class
    {
        ISubscriptionSelector<T> Subscribe(Action<ISubscriberConfigurator> configurator = null);
    }
}