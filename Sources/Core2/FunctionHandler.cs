using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class FunctionHandler : ICallHandler
    {
        private readonly Func<object, object> _handler;

        public FunctionHandler(Func<object, object> handler)
        {
            _handler = handler;
        }

        public RawBusMessage Dispatch(RawBusMessage message)
        {
            object result = _handler(message.Data);

            return new RawBusMessage {Data = result};
        }
    }
}