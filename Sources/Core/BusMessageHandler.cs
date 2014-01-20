using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    class BusMessageHandler<TData> : ICallHandler
    {
        private readonly Action<BusMessage<TData>> _action;

        public BusMessageHandler(Action<BusMessage<TData>> action)
        {
            _action = action;
        }

        public void Dispatch(RawBusMessage message)
        {
            BusMessage<TData> busMessage = new BusMessage<TData>
                {
                    BusId = message.BusId,
                    Sent = message.Sent,
                    Data = (TData)message.Data
                };

            foreach (BusHeader header in message.Headers)
            {
                busMessage.Headers.Add(header);
            }

            _action(busMessage);
        }
    }
}