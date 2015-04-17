using System;
using System.Collections.Generic;

using MessageBus.Core.API;

using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class Subscriber : SubscriberBase, ISubscriber
    {
        public Subscriber(IModel model, string queue, IMessageConsumer consumer, SubscriberConfigurator configurator)
            : base(model, queue, consumer, configurator)
        {
        }

        public bool Subscribe<TData>(Action<TData> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            return Subscribe(typeof(TData), o => callback((TData)o), hierarchy, filter);
        }

        public bool Subscribe(Type dataType, Action<object> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            ActionHandler actionHandler = new ActionHandler(callback);

            return _helper.Subscribe(dataType, actionHandler, hierarchy, filter);
        }

        public bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            return _helper.Subscribe(typeof(TData), new BusMessageHandler<TData>(o => callback((BusMessage<TData>)o)), hierarchy, filter);
        }

        public bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            return _helper.Subscribe(dataType, new RawHandler(callback), hierarchy, filter);
        }
    }
}