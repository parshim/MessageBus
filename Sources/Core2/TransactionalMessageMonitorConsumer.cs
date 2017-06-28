using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class TransactionalMessageMonitorConsumer : DefaultBasicConsumer
    {
        private readonly IMessageHelper _messageHelper;

        private readonly Action<RawBusMessage> _monitor;
        
        private readonly IExceptionFilter _exceptionFilter;

        public TransactionalMessageMonitorConsumer(IModel model, IMessageHelper messageHelper, Action<RawBusMessage> monitor, IExceptionFilter exceptionFilter):base(model)
        {
            _messageHelper = messageHelper;
            _monitor = monitor;
            _exceptionFilter = exceptionFilter;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            DataContractKey dataContractKey = properties.GetDataContractKey();
            
            RawBusMessage rawBusMessage = _messageHelper.ConstructMessage(dataContractKey, properties, (object)body);

            try
            {
                _monitor(rawBusMessage);

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
                bool requeue = _exceptionFilter.Filter(ex, rawBusMessage, redelivered, deliveryTag);

                Model.BasicNack(deliveryTag, false, requeue);
            }
        }
    }
}