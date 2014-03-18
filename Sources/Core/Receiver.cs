using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class Receiver : SubscriptionBase, IReceiver
    {
        private readonly ICallbackDispatcher _callbackDispatcher;

        public Receiver(IInputChannel inputChannel, IMessageFilter messageFilter, ICallbackDispatcher dispatcher)
            : base(inputChannel, messageFilter, dispatcher)
        {
            _callbackDispatcher = dispatcher;
        }

        public bool Subscribe<TData>(bool hierarchy = false, bool receiveSelfPublish = false, IEnumerable<BusHeader> filter = null)
        {
            return _callbackDispatcher.Subscribe(typeof (TData), new NullCallHandler(), hierarchy, receiveSelfPublish,
                                                 filter);
        }

        public TData Receive<TData>()
        {
            var rawBusMessage = ReceiveRawBusMessage();

            if (rawBusMessage == null) return default(TData);

            return (TData) rawBusMessage.Data;
        }
        
        public BusMessage<TData> ReceiveBusMessage<TData>()
        {
            var message = ReceiveRawBusMessage();

            if (message == null) return null;

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

            return busMessage;
        }

        private RawBusMessage ReceiveRawBusMessage()
        {
            Message message;

            if (!_inputChannel.TryReceive(TimeSpan.FromMilliseconds(100), out message))
            {
                return null;
            }

            RawBusMessage rawBusMessage;

            using (message)
            {
                rawBusMessage = _dispatcher.Translate(message);
            }

            return rawBusMessage;
        }
    }
}