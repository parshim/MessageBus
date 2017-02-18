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
            sendParams.BusMessage.Sent = DateTime.Now;
            sendParams.BusMessage.BusId = sendParams.BusId;

            BasicProperties basicProperties = new BasicProperties
            {
                AppId = sendParams.BusMessage.BusId,
                Timestamp = sendParams.BusMessage.Sent.ToAmqpTimestamp(),
                ContentType = sendParams.Serializer.ContentType,
                Headers = new Dictionary<string, object>()
            };

            byte[] bytes;
            
            if (sendParams.BusMessage.Data != null)
            {
                if (string.IsNullOrEmpty(sendParams.BusMessage.Name) || string.IsNullOrEmpty(sendParams.BusMessage.Namespace))
                {
                    Type type = sendParams.BusMessage.Data.GetType();
                    DataContractKey contractKey = _nameMappings.GetOrAdd(type, t => t.GetDataContractKey());

                    basicProperties.Type = contractKey.Name;
                    basicProperties.Headers.Add(MessagingConstants.HeaderNames.Name, contractKey.Name);
                    basicProperties.Headers.Add(MessagingConstants.HeaderNames.NameSpace, contractKey.Ns);

                    sendParams.BusMessage.Name = contractKey.Name;
                    sendParams.BusMessage.Namespace = contractKey.Ns;
                }                

                bytes = sendParams.Serializer.Serialize(sendParams.BusMessage);
            }
            else
            {
                bytes = new byte[0];
            }
            
            foreach (BusHeaderBase header in sendParams.BusMessage.Headers)
            {
                basicProperties.Headers.Add(header.Name, header.GetValue());
            }

            if (sendParams.PersistentDelivery)
            {
                basicProperties.Persistent = true;
            }

            if (!string.IsNullOrEmpty(sendParams.ReplyTo))
            {
                basicProperties.ReplyTo = sendParams.ReplyTo;
            }

            if (!string.IsNullOrEmpty(sendParams.CorrelationId))
            {
                basicProperties.CorrelationId = sendParams.CorrelationId;
            }

            if (sendParams.Priority.HasValue)
            {
                basicProperties.Priority = sendParams.Priority.Value;
            }

            sendParams.Model.BasicPublish(sendParams.Exchange, sendParams.RoutingKey, sendParams.MandatoryDelivery, basicProperties, bytes);
        }

        public Type GetDataType(DataContractKey dataContractKey)
        {
            return _nameMappings.Where(pair => pair.Value.Equals(dataContractKey)).Select(pair => pair.Key).FirstOrDefault();
        }
    }
}