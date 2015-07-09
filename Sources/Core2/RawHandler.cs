using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class RawHandler : ICallHandler
    {
        private readonly Action<RawBusMessage> _action;

        public RawHandler(Action<RawBusMessage> action)
        {
            _action = action;
        }

        public RawBusMessage Dispatch(RawBusMessage message)
        {
            _action(message);

            return null;
        }
    }
}