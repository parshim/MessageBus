using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    class RawHandler : ICallHandler
    {
        private readonly Action<RawBusMessage> _action;

        public RawHandler(Action<RawBusMessage> action)
        {
            _action = action;
        }

        public void Dispatch(RawBusMessage message)
        {
            _action(message);
        }
    }
}