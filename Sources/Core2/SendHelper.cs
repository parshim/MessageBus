using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class SendHelper : ISendHelper
    {
        private readonly ConcurrentDictionary<Type, DataContractKey> _nameMappings = new ConcurrentDictionary<Type, DataContractKey>();
        
        private void Send(byte[] bytes, string contentType, string name, string ns, BusMessage busMessage,  SendParams sendParams)
        {
            busMessage.Sent = DateTime.Now;
            busMessage.BusId = sendParams.BusId;

            var basicProperties = sendParams.Model.CreateBasicProperties();

            basicProperties.AppId = busMessage.BusId;
            basicProperties.Timestamp = busMessage.Sent.ToAmqpTimestamp();
            basicProperties.ContentType = contentType;
            basicProperties.Headers = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(name))
            {
                basicProperties.Type = name;

                basicProperties.Headers.Add(MessagingConstants.HeaderNames.Name, name);
            }

            if (!string.IsNullOrEmpty(ns))
            {
                basicProperties.Headers.Add(MessagingConstants.HeaderNames.NameSpace, ns);
            }

            foreach (BusHeaderBase header in busMessage.Headers)
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

        public void Send(SerializedBusMessage message, SendParams sendParams)
        {
            Send(message.Data, message.ContentType, message.Name, message.Namespace, message, sendParams);
        }

        public void Send(RawBusMessage message, ISerializer serializer, SendParams sendParams)
        {
            if (message.Data != null)
            {
                Type type = message.Data.GetType();
                DataContractKey contractKey = _nameMappings.GetOrAdd(type, t => t.GetDataContractKey());

                if (message.Name == null)
                {
                    message.Name = contractKey.Name;
                }

                if (message.Namespace == null)
                {
                    message.Namespace = contractKey.Ns;
                }
            }

            var bytes = message.Data as byte[];

            if (bytes == null)
            {
                bytes = message.Data != null ? serializer.Serialize(message) : new byte[0];
            }
            

            Send(bytes, serializer.ContentType, message.Name, message.Namespace, message, sendParams);
        }
    }
}