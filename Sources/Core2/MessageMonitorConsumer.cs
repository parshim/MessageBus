using System;
using System.Threading.Tasks;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class MessageMonitorConsumer : AsyncDefaultBasicConsumer
    {
        private readonly IMessageHelper _messageHelper;

        private readonly Action<SerializedBusMessage> _monitor;

        private readonly IErrorSubscriber _errorSubscriber;

        public MessageMonitorConsumer(IModel model, IMessageHelper messageHelper, Action<SerializedBusMessage> monitor, IErrorSubscriber errorSubscriber):base(model)
        {
            _messageHelper = messageHelper;
            _monitor = monitor;
            _errorSubscriber = errorSubscriber;
        }

        public override Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            DataContractKey dataContractKey = properties.GetDataContractKey();

            SerializedBusMessage message = _messageHelper.ConstructMessage(dataContractKey, properties, body);

            try
            {
                _monitor(message);
            }
            catch(Exception ex)
            {
                RawBusMessage raw = new RawBusMessage
                {
                    Data = message.Data,
                    Namespace = message.Namespace,
                    Name = message.Name,
                    BusId = message.BusId,
                    CorrelationId = message.CorrelationId,
                    Sent = message.Sent
                };

                foreach (var header in message.Headers)
                {
                    raw.Headers.Add(header);
                }

                _errorSubscriber.MessageDispatchException(raw, ex);
            }

            return Task.FromResult(0);
        }
    }
}