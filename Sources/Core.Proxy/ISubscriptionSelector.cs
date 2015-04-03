using System;
using System.Linq.Expressions;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public interface ISubscriptionSelector<TContract> : IDisposable
    {
        void Subscribe<TData>(Expression<Func<TContract, Action<TData>>> methodSelector, Action<TData> notificationCallback, params BusHeader[] filterHeaders);
        
        void Subscribe<TData>(Expression<Func<TContract, Action>> methodSelector, Action notificationCallback, params BusHeader[] filterHeaders);
    }
}