using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal sealed class Subscriber : MessagePumpSubscriptionBase, ISubscriber
    {
        private readonly ICallbackDispatcher _callbackDispatcher;

        public Subscriber(IInputChannel inputChannel, IMessageFilter messageFilter, ICallbackDispatcher dispatcher)
            : base(inputChannel, messageFilter, dispatcher)
        {
            _callbackDispatcher = dispatcher;
        }
        
        public bool Subscribe<TData>(Action<TData> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            return Subscribe(typeof(TData), o => callback((TData)o), hierarchy, filter);
        }

        public bool Subscribe(Type dataType, Action<object> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            ActionHandler actionHandler = new ActionHandler(callback);

            return _callbackDispatcher.Subscribe(dataType, actionHandler, hierarchy, false, filter);
        }

        public bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            return _callbackDispatcher.Subscribe(typeof(TData), new BusMessageHandler<TData>(o => callback((BusMessage<TData>) o)), hierarchy, false, filter);
        }

        public bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            return _callbackDispatcher.Subscribe(dataType, new RawHandler(callback), hierarchy, filter);
        }

    }
}