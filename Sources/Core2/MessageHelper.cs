using System.Collections.Generic;
using System.Text;
using MessageBus.Core;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class MessageHelper : IMessageHelper
    {
        public RawBusMessage ConstructMessage(DataContractKey dataContractKey, IBasicProperties properties, object data)
        {
            RawBusMessage message = new RawBusMessage
            {
                Data = data,
                BusId = properties.AppId,
                Sent = properties.Timestamp.GetDateTime(),
                Name = dataContractKey.Name,
                Namespace = dataContractKey.Ns
            };

            foreach (KeyValuePair<string, object> header in properties.Headers)
            {
                object o = header.Value;

                message.Headers.Add(new BusHeader(header.Key, Encoding.ASCII.GetString((byte[])o)));
            }

            return message;
        }
        
        public BusMessage<T> ConstructMessage<T>(DataContractKey dataContractKey, IBasicProperties properties, T data)
        {
            BusMessage<T> message = new BusMessage<T>
            {
                Data = data,
                BusId = properties.AppId,
                Sent = properties.Timestamp.GetDateTime()
            };

            foreach (KeyValuePair<string, object> header in properties.Headers)
            {
                object o = header.Value;

                message.Headers.Add(new BusHeader(header.Key, Encoding.ASCII.GetString((byte[])o)));
            }

            return message;
        }
    }
}