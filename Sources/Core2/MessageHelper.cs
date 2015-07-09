using System.Collections.Generic;
using System.Text;
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

            if (properties.IsCorrelationIdPresent())
            {
                message.CorrelationId = properties.CorrelationId;
            }

            ConstructHeaders(message, properties);

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

            if (properties.IsCorrelationIdPresent())
            {
                message.CorrelationId = properties.CorrelationId;
            }

            ConstructHeaders(message, properties);

            return message;
        }

        private void ConstructHeaders(BusMessage message, IBasicProperties properties)
        {
            foreach (KeyValuePair<string, object> header in properties.Headers)
            {
                object o = header.Value;

                if (header.Key == XDeadHeader.WellknownName)
                {
                    List<object> list = (List<object>) o;

                    Dictionary<string, object> values = (Dictionary<string, object>)list[0];

                    byte[] reason = (byte[])values["reason"];
                    byte[] queue = (byte[])values["queue"];
                    AmqpTimestamp time = (AmqpTimestamp)values["time"];
                    byte[] exchange = (byte[])values["exchange"];
                    List<object> routingKeys = (List<object>)values["routing-keys"];

                    XDeadHeader xDeadHeader = new XDeadHeader
                    {
                        Reason = Encoding.ASCII.GetString(reason),
                        Queue = Encoding.ASCII.GetString(queue),
                        Exchange = Encoding.ASCII.GetString(exchange),
                        Time = time.GetDateTime()
                    };

                    foreach (var routingKey in routingKeys)
                    {
                        xDeadHeader.RoutingKeys.Add(Encoding.ASCII.GetString((byte[])routingKey));
                    }

                    message.Headers.Add(xDeadHeader);
                }
                else if (header.Key == RejectedHeader.WellknownName)
                {
                    message.Headers.Add(new RejectedHeader());
                }
                else if (header.Key == ExceptionHeader.WellknownName)
                {
                    message.Headers.Add(new ExceptionHeader
                    {
                        Message = Encoding.ASCII.GetString((byte[])o)
                    });
                }
                else
                {
                    message.Headers.Add(new BusHeader(header.Key, Encoding.ASCII.GetString((byte[])o)));
                }
            }
        } 
    }
}