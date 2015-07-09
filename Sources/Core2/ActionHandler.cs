using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class ActionHandler<TData> : ICallHandler
    {
        private readonly Action<TData> _action;

        public ActionHandler(Action<TData> action)
        {
            _action = action;
        }

        public RawBusMessage Dispatch(RawBusMessage message)
        {
            _action((TData)message.Data);

            return new RawBusMessage();
        }
    }
}
