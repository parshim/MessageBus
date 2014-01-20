using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    class ActionHandler : ICallHandler
    {
        private readonly Action<object> _action;

        public ActionHandler(Action<object> action)
        {
            _action = action;
        }

        public void Dispatch(RawBusMessage message)
        {
            _action(message.Data);
        }
    }
}
