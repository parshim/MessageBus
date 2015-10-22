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

        public void Send<TData>(TData data, bool persistant)
        {
            Send(new RawBusMessage { Data = data }, persistant);
        }

        public void Send<TData>(BusMessage<TData> busMessage, bool persistant)
        {
            RawBusMessage rawBusMessage = new RawBusMessage
            {
                Data = busMessage.Data
            };

            foreach (var header in busMessage.Headers)
            {
                rawBusMessage.Headers.Add(header);
            }

            Send(rawBusMessage, persistant);
        }

        public void Send(RawBusMessage busMessage, bool persistant)
        {
            foreach (var header in _configuration.Headers)
            {
                busMessage.Headers.Add(header);
            }
            
            _sendHelper.Send(new SendParams
            {
                BusId = _busId,
                Model = _model,
                BusMessage = busMessage,
                Serializer = _configuration.Serializer,
                CorrelationId = "",
                Exchange = _configuration.Exchange,
                MandatoryDelivery = _configuration.MandatoryDelivery,
                PersistentDelivery = persistant || _configuration.PersistentDelivery,
                RoutingKey = _configuration.RoutingKey,
                ReplyTo = _configuration.ReplyTo
            });
        }
    }
}