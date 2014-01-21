using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal sealed class Subscriber : SubscriberBase, ISubscriber
    {
        private readonly ICallbackDispatcher _dispatcher;

        internal Subscriber(IInputChannel inputChannel, IMessageFilter messageFilter, ICallbackDispatcher dispatcher)
            : base(inputChannel, messageFilter, dispatcher)
        {
            _dispatcher = dispatcher;
        }
        
        public bool Subscribe<TData>(Action<TData> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            return Subscribe(typeof(TData), o => callback((TData)o), hierarchy, receiveSelfPublish, filter);
        }

        public bool Subscribe(Type dataType, Action<object> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            ActionHandler actionHandler = new ActionHandler(callback);

            return _dispatcher.Subscribe(dataType, actionHandler, hierarchy, receiveSelfPublish, filter);
        }

        public bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            return _dispatcher.Subscribe(typeof(TData), new BusMessageHandler<TData>(o => callback((BusMessage<TData>) o)), hierarchy, receiveSelfPublish, filter);
        }

        public bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            return _dispatcher.Subscribe(dataType, new RawHandler(callback), hierarchy, receiveSelfPublish, filter);
        }

    }
}