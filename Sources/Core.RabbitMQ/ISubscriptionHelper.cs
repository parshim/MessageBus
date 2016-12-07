using System;
using System.Collections.Generic;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public interface ISubscriptionHelper
    {
        bool Subscribe(Type dataType, ICallHandler handler, bool hierarchy, IEnumerable<BusHeader> filter);
        bool Subscribe(Type dataType, ICallHandler handler, IEnumerable<BusHeader> filter);
        bool SubscribeHierarchy(Type baseType, ICallHandler handler, IEnumerable<BusHeader> filter);
        void RegisterSubscription(object instance);
    }
}