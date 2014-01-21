using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;

namespace MessageBus.Core.API
{
    internal interface IDispatcher
    {
        IEnumerable<MessageFilterInfo> GetApplicableFilters();

        void Dispatch(Message message);
    }

    internal interface ICallbackDispatcher : IDispatcher
    {
        bool Subscribe(Type dataType, ICallHandler handler, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter);
        bool Subscribe(Type dataType, ICallHandler handler, bool receiveSelfPublish, IEnumerable<BusHeader> filter);
        bool SubscribeHierarchy(Type baseType, ICallHandler handler, bool receiveSelfPublish, IEnumerable<BusHeader> filter);
    }

    internal interface ISubscriptionDispatcher : IDispatcher
    {
        void RegisterSubscribtion(object instance);
    }
}