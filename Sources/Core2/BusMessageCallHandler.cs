using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class BusMessageCallHandler<TData> : ICallHandler
    {
        private readonly Action<BusMessage<TData>> _action;

        public BusMessageCallHandler(Action<BusMessage<TData>> action)
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

            _action(busMessage);

            return null;
        }
    }
}