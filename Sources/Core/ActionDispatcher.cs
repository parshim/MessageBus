using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    class ActionDispatcher : IDispatcher
    {
        private readonly Action<object> _action;

        public ActionDispatcher(Action<object> action)
        {
            _action = action;
        }

        public void Dispatch(RawBusMessage message)
        {
            _action(message.Data);
        }
    }
    
    class RawDispatcher : IDispatcher
    {
        private readonly Action<RawBusMessage> _action;

        public RawDispatcher(Action<RawBusMessage> action)
        {
            _action = action;
        }

        public void Dispatch(RawBusMessage message)
        {
            _action(message);
        }
    }

    class BusMessageDispatcher<TData> : IDispatcher
    {
        private readonly Action<BusMessage<TData>> _action;

        public BusMessageDispatcher(Action<BusMessage<TData>> action)
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
