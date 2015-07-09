using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Core.API;
using RabbitMQ.Client.Framing;

namespace MessageBus.Core
{
    public class SendHelper : ISendHelper
    {
        private readonly ConcurrentDictionary<Type, DataContractKey> _nameMappings = new ConcurrentDictionary<Type, DataContractKey>();
        
        public void Send(SendParams sendParams)
        {
            Type type = sendParams.BusMessage.Data.GetType();

            DataContractKey contractKey = _nameMappings.GetOrAdd(type, t => t.GetDataContractKey());
            
            sendParams.BusMessage.Sent = DateTime.Now;
            sendParams.BusMessage.BusId = sendParams.BusId;

            BasicProperties basicProperties = new BasicProperties
            {
                AppId = sendParams.BusMessage.BusId,
                Timestamp = sendParams.BusMessage.Sent.ToAmqpTimestamp(),
                Type = contractKey.Name,
                ContentType = sendParams.Serializer.ContentType,
                Headers = new Dictionary<string, object>
                {
                    {MessagingConstants.HeaderNames.Name, contractKey.Name},
                    {MessagingConstants.HeaderNames.NameSpace, contractKey.Ns}
                }
            };

            foreach (BusHeaderBase header in sendParams.BusMessage.Headers)
            {
                basicProperties.Headers.Add(header.Name, header.GetValue());
            }

            if (sendParams.PersistentDelivery)
            {
                basicProperties.SetPersistent(true);
            }

            if (!string.IsNullOrEmpty(sendParams.ReplyTo))
            {
                basicProperties.ReplyTo = sendParams.ReplyTo;
            }

            if (!string.IsNullOrEmpty(sendParams.CorrelationId))
            {
                basicProperties.CorrelationId = sendParams.CorrelationId;
            }

            byte[] bytes = sendParams.Serializer.Serialize(sendParams.BusMessage);

            sendParams.Model.BasicPublish(sendParams.Exchange, sendParams.RoutingKey, sendParams.MandatoryDelivery, false, basicProperties, bytes);
        }

        public Type GetDataType(DataContractKey dataContractKey)
        {
            return _nameMappings.Where(pair => pair.Value.Equals(dataContractKey)).Select(pair => pair.Key).FirstOrDefault();
        }
    }
}