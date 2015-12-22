using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public interface ISubscriptionSelector<TContract> : IDisposable
    {
        void Subscribe<TData>(Expression<Func<TContract, Action<TData>>> methodSelector, Action<TData> notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders);

        void Subscribe(Expression<Func<TContract, Action>> methodSelector, Action notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders);

        void Subscribe<TData>(Expression<Func<TContract, Action<TData>>> methodSelector, Action<TData, IEnumerable<BusHeader>> notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders);

        void Subscribe(Expression<Func<TContract, Action>> methodSelector, Action<IEnumerable<BusHeader>> notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders);
    }
}