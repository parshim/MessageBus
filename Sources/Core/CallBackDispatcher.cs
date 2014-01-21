using System;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    class CallBackDispatcher : DispatcherBase, ICallbackDispatcher
    {
        public CallBackDispatcher(IErrorSubscriber errorSubscriber, string busId) : base(errorSubscriber, busId)
        {
        }

        public bool Subscribe(Type dataType, ICallHandler handler, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            if (hierarchy)
            {
                return SubscribeHierarchy(dataType, handler, receiveSelfPublish, filter);
            }

            return Subscribe(dataType, handler, receiveSelfPublish, filter);
        }

        public bool Subscribe(Type dataType, ICallHandler handler, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            DataContract dataContract = new DataContract(dataType);

            return _registeredTypes.TryAdd(dataContract.Key,
                                           new MessageSubscribtionInfo(dataContract.Key, handler,
                                                                       dataContract.Serializer, receiveSelfPublish,
                                                                       filter ?? Enumerable.Empty<BusHeader>()));
        }

        public bool SubscribeHierarchy(Type baseType, ICallHandler handler, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            var types = from type in baseType.Assembly.GetTypes()
                        where type != baseType && baseType.IsAssignableFrom(type)
                        select type;

            bool atLeastOne = false;

            foreach (Type type in types)
            {
                atLeastOne = Subscribe(type, handler, receiveSelfPublish, filter) || atLeastOne;
            }

            return atLeastOne;
        }

    }
}