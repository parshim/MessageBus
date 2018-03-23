using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class TransactionalMessageMonitorConsumer : DefaultBasicConsumer
    {
        private readonly IMessageHelper _messageHelper;

        private readonly Action<SerializedBusMessage> _monitor;
        
        private readonly IExceptionFilter _exceptionFilter;

        public TransactionalMessageMonitorConsumer(IModel model, IMessageHelper messageHelper, Action<SerializedBusMessage> monitor, IExceptionFilter exceptionFilter):base(model)
        {
            _messageHelper = messageHelper;
            _monitor = monitor;
            _exceptionFilter = exceptionFilter;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            DataContractKey dataContractKey = properties.GetDataContractKey();

            SerializedBusMessage message = _messageHelper.ConstructMessage(dataContractKey, properties, body);

            try
            {
                _monitor(message);

                Model.BasicAck(deliveryTag, false);
            }
            catch (RejectMessageException)
            {
                // If reject message exception is thrown -> reject message without requeue it. 
                // Message will be lost or transfered to dead letter exchange by broker
                Model.BasicNack(deliveryTag, false, false);
            }
            catch (Exception ex)
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

                bool requeue = _exceptionFilter.Filter(ex, raw, redelivered, deliveryTag);

                Model.BasicNack(deliveryTag, false, requeue);
            }
        }
    }
}