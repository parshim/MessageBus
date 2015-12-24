using System;
using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class AsyncBusMessageCallHandler<TData> : ICallHandler
    {
        private readonly Func<BusMessage<TData>, Task> _action;

        public AsyncBusMessageCallHandler(Func<BusMessage<TData>, Task> action)
        {
            _action = action;
        }

        public async Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            BusMessage<TData> busMessage = new BusMessage<TData>
                {
                    BusId = message.BusId,
                    Sent = message.Sent,
                    Data = (TData)message.Data
                };

            foreach (var header in message.Headers)
            {
                busMessage.Headers.Add(header);
            }

            await _action(busMessage);

            return new RawBusMessage();
        }
    }
    
    public class BusMessageCallHandler<TData> : ICallHandler
    {
        private readonly Action<BusMessage<TData>> _action;

        public BusMessageCallHandler(Action<BusMessage<TData>> action)
        {
            _action = action;
        }

        public Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            BusMessage<TData> busMessage = new BusMessage<TData>
                {
                    BusId = message.BusId,
                    Sent = message.Sent,
                    Data = (TData)message.Data
                };

            foreach (var header in message.Headers)
            {
                busMessage.Headers.Add(header);
            }

            _action(busMessage);

            return Task.FromResult(new RawBusMessage());
        }
    }
}