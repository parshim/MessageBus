using System;
using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class AsyncRawHandler : ICallHandler
    {
        private readonly Func<RawBusMessage, Task> _action;

        public AsyncRawHandler(Func<RawBusMessage, Task> action)
        {
            _action = action;
        }

        public async Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            await _action(message);

            return new RawBusMessage();
        }
    }
    public class RawHandler : ICallHandler
    {
        private readonly Action<RawBusMessage> _action;

        public RawHandler(Action<RawBusMessage> action)
        {
            _action = action;
        }

        public Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            _action(message);

            return Task.FromResult(new RawBusMessage());
        }
    }
}