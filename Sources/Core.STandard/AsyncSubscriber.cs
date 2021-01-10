using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class AsyncSubscriber : SubscriberBase, IAsyncSubscriber
    {
        private readonly ISubscriptionHelper _helper;

        public AsyncSubscriber(IModel model, string queue, IMessageConsumer consumer, ISubscriptionHelper helper, SubscriberConfigurator configurator)
            : base(model, queue, consumer, configurator)
        {
            _helper = helper;
        }

        public bool Subscribe<TData>(Func<TData, Task> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            AsyncActionHandler<TData> asyncActionHandler = new AsyncActionHandler<TData>(callback);

            return _helper.Subscribe(typeof(TData), asyncActionHandler, hierarchy, filter);
        }

        public bool Subscribe(Type dataType, Func<object, Task> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            AsyncActionHandler<object> asyncActionHandler = new AsyncActionHandler<object>(callback);

            return _helper.Subscribe(dataType, asyncActionHandler, hierarchy, filter);
        }

        public bool Subscribe<TData>(Func<BusMessage<TData>, Task> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            AsyncBusMessageCallHandler<TData> handler = new AsyncBusMessageCallHandler<TData>(callback);

            return _helper.Subscribe(typeof(TData), handler, hierarchy, filter);
        }

        public bool Subscribe<TData, TReplyData>(Func<BusMessage<TData>, Task<BusMessage<TReplyData>>> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null)
        {
            AsyncBusMessageCallHandler<TData, TReplyData> handler = new AsyncBusMessageCallHandler<TData, TReplyData>(callback);

            return _helper.Subscribe(typeof(TData), handler, hierarchy, filter);
        }

        public bool Subscribe<TData, TReplyData>(Func<TData, Task<TReplyData>> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null)
        {
            AsyncFunctionHandler<TData, TReplyData> handler = new AsyncFunctionHandler<TData, TReplyData>(callback);

            return _helper.Subscribe(typeof (TData), handler, hierarchy, filter);
        }

        public bool Subscribe(Type dataType, Func<RawBusMessage, Task> callback, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            AsyncRawHandler handler = new AsyncRawHandler(callback);

            return _helper.Subscribe(dataType, handler, hierarchy, filter);
        }
    }
}