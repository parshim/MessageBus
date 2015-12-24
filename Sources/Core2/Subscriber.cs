using System;
using System.Collections.Generic;
using MessageBus.Core.API;

using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class Subscriber : SubscriberBase, ISubscriber
    {
        private readonly ISubscriptionHelper _helper;

        public Subscriber(IModel model, string queue, IMessageConsumer consumer, ISubscriptionHelper helper, SubscriberConfigurator configurator)
            : base(model, queue, consumer, configurator)
        {
            _helper = helper;
        }

        public bool Subscribe<TData>(Action<TData> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            ActionHandler<TData> asyncActionHandler = new ActionHandler<TData>(callback);

            return _helper.Subscribe(typeof(TData), asyncActionHandler, hierarchy, filter);
        }
        
        public bool Subscribe(Type dataType, Action<object> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            ActionHandler<object> asyncActionHandler = new ActionHandler<object>(callback);

            return _helper.Subscribe(dataType, asyncActionHandler, hierarchy, filter);
        }

        public bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            BusMessageCallHandler<TData> handler = new BusMessageCallHandler<TData>(callback);

            return _helper.Subscribe(typeof(TData), handler, hierarchy, filter);
        }

        public bool Subscribe<TData, TReplyData>(Func<BusMessage<TData>, BusMessage<TReplyData>> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null)
        {
            BusMessageCallHandler<TData, TReplyData> handler = new BusMessageCallHandler<TData, TReplyData>(callback);

            return _helper.Subscribe(typeof(TData), handler, hierarchy, filter);
        }

        public bool Subscribe<TData, TReplyData>(Func<TData, TReplyData> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null)
        {
            FunctionHandler<TData, TReplyData> handler = new FunctionHandler<TData, TReplyData>(callback);

            return _helper.Subscribe(typeof (TData), handler, hierarchy, filter);
        }

        public bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            RawHandler handler = new RawHandler(callback);

            return _helper.Subscribe(dataType, handler, hierarchy, filter);
        }
    }
}