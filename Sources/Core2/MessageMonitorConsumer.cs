using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class MessageMonitorConsumer : DefaultBasicConsumer
    {
        private readonly IMessageHelper _messageHelper;

        private readonly Action<RawBusMessage> _monitor;

        private readonly IErrorSubscriber _errorSubscriber;

        public MessageMonitorConsumer(IModel model, IMessageHelper messageHelper, Action<RawBusMessage> monitor, IErrorSubscriber errorSubscriber):base(model)
        {
            _messageHelper = messageHelper;
            _monitor = monitor;
            _errorSubscriber = errorSubscriber;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            DataContractKey dataContractKey = properties.GetDataContractKey();

            RawBusMessage rawBusMessage = _messageHelper.ConstructMessage(dataContractKey, properties, (object)body);

            try
            {
                _monitor(rawBusMessage);
            }
            catch(Exception ex)
            {
                _errorSubscriber.MessageDispatchException(rawBusMessage, ex);
            }
        }
    }
}