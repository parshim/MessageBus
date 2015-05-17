using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class BusMessageHandler<TData> : ICallHandler
    {
        private readonly Action<object> _action;

        public BusMessageHandler(Action<object> action)
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

            foreach (var header in message.Headers)
            {
                busMessage.Headers.Add(header);
            }

            _action(busMessage);
        }
    }
}