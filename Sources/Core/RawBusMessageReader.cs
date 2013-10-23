using System;
using System.Linq;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class RawBusMessageReader
    {
        public RawBusMessage ReadMessage(Message message, Action<RawBusMessage, XmlDictionaryReader> deserializer)
        {
            string busId = message.Headers.GetHeader<string>(MessagingConstancts.HeaderNames.BusId,
                                                             MessagingConstancts.Namespace.MessageBus,
                                                             MessagingConstancts.Actor.Bus);

            DateTime sent = message.Headers.GetHeader<DateTime>(MessagingConstancts.HeaderNames.SentTime,
                                                                MessagingConstancts.Namespace.MessageBus,
                                                                MessagingConstancts.Actor.Bus);

            RawBusMessage rawBusMessage = new RawBusMessage
                {
                    BusId = busId,
                    Sent = sent
                };

            foreach (MessageHeaderInfo headerInfo in message.Headers.Where(info => info.Actor == MessagingConstancts.Actor.User &&
                                                                                   info.Namespace == MessagingConstancts.Namespace.MessageBus))
            {
                string value = message.Headers.GetHeader<string>(headerInfo.Name, headerInfo.Namespace, headerInfo.Actor);

                rawBusMessage.Headers.Add(new BusHeader
                    {
                        Name = headerInfo.Name,
                        Value = value
                    });
            }

            using (XmlDictionaryReader bodyContents = message.GetReaderAtBodyContents())
            {
                rawBusMessage.Name = bodyContents.Name;
                rawBusMessage.Namespace = bodyContents.NamespaceURI;

                deserializer(rawBusMessage, bodyContents);
            }

            return rawBusMessage;
        }
    }
}