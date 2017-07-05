using System;
using MessageBus.Core.API;

using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class Publisher : PublisherBase, IPublisher
    {
        public Publisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper) : base(model, busId, configuration, messageHelper, sendHelper)
        {
        }

        protected override void OnMessageReturn(int replyCode, string replyText, RawBusMessage message)
        {
            _configuration.ErrorHandler.DeliveryFailed(replyCode, replyText, message);
        }

        public void Send<TData>(TData data)
        {
            SendInternal(new RawBusMessage { Data = data }, false, null);
        }

        public void Send<TData>(TData data, bool persistant)
        {
            SendInternal(new RawBusMessage { Data = data }, persistant, null);
        }

        public void Send<TData>(TData data, bool persistant, byte priority)
        {
            SendInternal(new RawBusMessage { Data = data }, persistant, priority);
        }

        public void Send<TData>(BusMessage<TData> busMessage)
        {
            RawBusMessage rawBusMessage = busMessage.ToRawBusMessage();

            SendInternal(rawBusMessage, false, null);
        }

        public void Send<TData>(BusMessage<TData> busMessage, bool persistant)
        {
            RawBusMessage rawBusMessage = busMessage.ToRawBusMessage();

            SendInternal(rawBusMessage, persistant, null);
        }

        public void Send<TData>(BusMessage<TData> busMessage, bool persistant, byte priority)
        {
            RawBusMessage rawBusMessage = busMessage.ToRawBusMessage();

            SendInternal(rawBusMessage, persistant, priority);
        }

        public void Send(RawBusMessage busMessage)
        {
            SendInternal(busMessage, false, null);
        }

        public void Send(RawBusMessage busMessage, bool persistant)
        {
            SendInternal(busMessage, persistant, null);
        }

        public void Send(RawBusMessage busMessage, bool persistant, byte priority)
        {
            SendInternal(busMessage, persistant, priority);
        }

        public void Send(SerializedBusMessage busMessage)
        {
            SendInternal(busMessage, false, null);
        }

        public void Send(SerializedBusMessage busMessage, bool persistant)
        {
            SendInternal(busMessage, persistant, null);
        }

        public void Send(SerializedBusMessage busMessage, bool persistant, byte priority)
        {
            SendInternal(busMessage, persistant, priority);
        }

        private void SendInternal(RawBusMessage busMessage, bool persistant, byte? priority)
        {
            foreach (var header in _configuration.Headers)
            {
                busMessage.Headers.Add(header);
            }
            
            _sendHelper.Send(busMessage, _configuration.Serializer, new SendParams
            {
                BusId = _busId,
                Model = _model,
                CorrelationId = "",
                Exchange = _configuration.Exchange,
                MandatoryDelivery = _configuration.MandatoryDelivery,
                PersistentDelivery = persistant || _configuration.PersistentDelivery,
                RoutingKey = _configuration.RoutingKey,
                ReplyTo = _configuration.ReplyTo,
                Priority = priority
            });

            _configuration.Trace.MessageSent(_busId, busMessage);
        }

        private void SendInternal(SerializedBusMessage busMessage, bool persistant, byte? priority)
        {
            foreach (var header in _configuration.Headers)
            {
                busMessage.Headers.Add(header);
            }
            
            _sendHelper.Send(busMessage, new SendParams
            {
                BusId = _busId,
                Model = _model,
                CorrelationId = "",
                Exchange = _configuration.Exchange,
                MandatoryDelivery = _configuration.MandatoryDelivery,
                PersistentDelivery = persistant || _configuration.PersistentDelivery,
                RoutingKey = _configuration.RoutingKey,
                ReplyTo = _configuration.ReplyTo,
                Priority = priority
            });

            RawBusMessage raw = new RawBusMessage
            {
                Data = busMessage.Data,
                Namespace = busMessage.Namespace,
                Name = busMessage.Name,
                BusId = busMessage.BusId,
                CorrelationId = busMessage.CorrelationId,
                Sent = busMessage.Sent
            };

            foreach (var header in busMessage.Headers)
            {
                raw.Headers.Add(header);
            }

            _configuration.Trace.MessageSent(_busId, raw);
        }

        public bool Blocked
        {
            get { return _configuration.Blocked; }
        }
    }
}