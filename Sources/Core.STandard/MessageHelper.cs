using System.Collections.Generic;
using System.Text;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class MessageHelper : IMessageHelper
    {
        public SerializedBusMessage ConstructMessage(DataContractKey dataContractKey, IBasicProperties properties, byte[] data)
        {
            SerializedBusMessage message = new SerializedBusMessage
            {
                Data = data,
                BusId = properties.AppId,
                Sent = properties.Timestamp.GetDateTime(),
                Name = dataContractKey.Name,
                Namespace = dataContractKey.Ns,
                ContentType = properties.ContentType
            };

            if (properties.IsCorrelationIdPresent())
            {
                message.CorrelationId = properties.CorrelationId;
            }

            ConstructHeaders(message, properties);

            return message;
        }

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

                switch (header.Key)
                {
                    case XDeadHeader.WellknownName:
                        message.Headers.Add(BuildXDeadHeader(o));
                        break;
                    case XReceivedFromHeader.WellknownName:
                        message.Headers.Add(BuildXReceivedFromHeader(o));
                        break;
                    case RejectedHeader.WellknownName:
                        message.Headers.Add(new RejectedHeader());
                        break;
                    case ExceptionHeader.WellknownName:
                        message.Headers.Add(new ExceptionHeader
                        {
                            Message = BuildHeaderValue(o)
                        });
                        break;
                    default:
                        message.Headers.Add(new BusHeader(header.Key, BuildHeaderValue(o)));
                        break;
                }
            }
        }

        private static string BuildHeaderValue(object o)
        {
            byte[] data = o as byte[];

            if (data == null) return null;

            return Encoding.UTF8.GetString((byte[])o);
        }

        private static XDeadHeader BuildXDeadHeader(object o)
        {
            List<object> list = (List<object>) o;

            Dictionary<string, object> values = (Dictionary<string, object>) list[0];

            byte[] reason = (byte[]) values["reason"];
            byte[] queue = (byte[]) values["queue"];
            AmqpTimestamp time = (AmqpTimestamp) values["time"];
            byte[] exchange = (byte[]) values["exchange"];
            List<object> routingKeys = (List<object>) values["routing-keys"];

            XDeadHeader xDeadHeader = new XDeadHeader
            {
                Reason = Encoding.ASCII.GetString(reason),
                Queue = Encoding.ASCII.GetString(queue),
                Exchange = Encoding.ASCII.GetString(exchange),
                Time = time.GetDateTime()
            };

            foreach (var routingKey in routingKeys)
            {
                xDeadHeader.RoutingKeys.Add(Encoding.ASCII.GetString((byte[]) routingKey));
            }

            return xDeadHeader;
        }

        private static XReceivedFromHeader BuildXReceivedFromHeader(object o)
        {
            List<object> list = (List<object>) o;

            Dictionary<string, object> values = (Dictionary<string, object>) list[0];

            byte[] uri = (byte[])values["uri"];
            byte[] exchange = (byte[])values["exchange"];
            bool redelivered = (bool)values["redelivered"];
            byte[] clusterName = (byte[])values["cluster-name"];

            XReceivedFromHeader xDeadHeader = new XReceivedFromHeader
            {
                Uri = Encoding.ASCII.GetString(uri),
                Exchange = Encoding.ASCII.GetString(exchange),
                ClusterName = Encoding.ASCII.GetString(clusterName),
                Redelivered = redelivered
            };

            return xDeadHeader;
        }
    }
}