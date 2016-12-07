using System;
using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class AsyncFunctionHandler<TIn, TOut> : ICallHandler
    {
        private readonly Func<TIn, Task<TOut>> _handler;

        public AsyncFunctionHandler(Func<TIn, Task<TOut>> handler)
        {
            _handler = handler;
        }

        public async Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            TOut result = await _handler((TIn) message.Data);

            return new RawBusMessage {Data = result};
        }
    }
    
    public class FunctionHandler<TIn, TOut> : ICallHandler
    {
        private readonly Func<TIn, TOut> _handler;

        public FunctionHandler(Func<TIn, TOut> handler)
        {
            _handler = handler;
        }

        public Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            TOut result = _handler((TIn) message.Data);

            return Task.FromResult(new RawBusMessage {Data = result});
        }
    }
}