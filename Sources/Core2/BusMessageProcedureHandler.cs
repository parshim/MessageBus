using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class BusMessageCallHandler<TData, TReplyData> : ICallHandler
    {
        private readonly Func<BusMessage<TData>, BusMessage<TReplyData>> _action;

        public BusMessageCallHandler(Func<BusMessage<TData>, BusMessage<TReplyData>> action)
        {
            _action = action;
        }

        public RawBusMessage Dispatch(RawBusMessage message)
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

            RawBusMessage replyMessage = new RawBusMessage
            {
                Data = busReplyMessage.Data
            };

            foreach (var header in busReplyMessage.Headers)
            {
                replyMessage.Headers.Add(header);
            }

            return replyMessage;
        }
    }
}