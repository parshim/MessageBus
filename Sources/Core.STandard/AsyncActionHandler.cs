using System;
using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class AsyncActionHandler<TData> : ICallHandler
    {
        private readonly Func<TData, Task> _action;

        public AsyncActionHandler(Func<TData, Task> action)
        {
            _action = action;
        }

        public async Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            await _action((TData)message.Data);

            return new RawBusMessage();
        }
    }
    
    public class ActionHandler<TData> : ICallHandler
    {
        private readonly Action<TData> _action;

        public ActionHandler(Action<TData> action)
        {
            _action = action;
        }

        public Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            _action((TData)message.Data);

            return Task.FromResult(new RawBusMessage());
        }
    }
}
