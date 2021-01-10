using System;
using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class AsyncBusMessageCallHandler<TData, TReplyData> : ICallHandler
    {
        private readonly Func<BusMessage<TData>, Task<BusMessage<TReplyData>>> _action;

        public AsyncBusMessageCallHandler(Func<BusMessage<TData>, Task<BusMessage<TReplyData>>> action)
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

            BusMessage<TReplyData> busReplyMessage = await _action(busMessage);

            RawBusMessage replyMessage = busReplyMessage.ToRawBusMessage();

            return replyMessage;
        }
    }
    
    public class BusMessageCallHandler<TData, TReplyData> : ICallHandler
    {
        private readonly Func<BusMessage<TData>, BusMessage<TReplyData>> _action;

        public BusMessageCallHandler(Func<BusMessage<TData>, BusMessage<TReplyData>> action)
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

            BusMessage<TReplyData> busReplyMessage = _action(busMessage);

            RawBusMessage replyMessage = busReplyMessage.ToRawBusMessage();

            return Task.FromResult(replyMessage);
        }
    }
}