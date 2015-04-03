using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class SoapSerializer : ISerializer
    {
        public string ContentType { get { return "application/soap+xml; charset=utf-8"; } }
        
        private readonly MessageEncoderFactory _encoderFactory;

        public SoapSerializer()
        {
            TextMessageEncodingBindingElement element = new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);

            _encoderFactory = element.CreateMessageEncoderFactory();
        }
        
        public byte[] Serialize(RawBusMessage busMessage)
        {
            using (Message message = Message.CreateMessage(_encoderFactory.MessageVersion, MessagingConstants.MessageAction.Regular, busMessage.Data))
            {
                SetBusHeaders(busMessage, message);

                SetUserHeaders(busMessage, message);

                // Serialize the message to stream
                using (MemoryStream str = new MemoryStream())
                {
                    _encoderFactory.Encoder.WriteMessage(message, str);

                    return str.ToArray();
                }
            }
        }

        private static void SetUserHeaders(RawBusMessage busMessage, Message message)
        {
            foreach (BusHeader busHeader in busMessage.Headers)
            {
                message.Headers.Add(MessageHeader.CreateHeader(busHeader.Name,
                                                               MessagingConstants.Namespace.MessageBus,
                                                               busHeader.Value, false,
                                                               MessagingConstants.Actor.User));
            }
        }

        private void SetBusHeader(Message message, string name, object value)
        {
            message.Headers.Add(MessageHeader.CreateHeader(name, MessagingConstants.Namespace.MessageBus,
                                                           value, false, MessagingConstants.Actor.Bus));
        }

        private void SetBusHeaders(RawBusMessage busMessage, Message message)
        {
            SetBusHeader(message, MessagingConstants.HeaderNames.Name, busMessage.Name);
            SetBusHeader(message, MessagingConstants.HeaderNames.NameSpace, busMessage.Namespace);
            SetBusHeader(message, MessagingConstants.HeaderNames.BusId, busMessage.BusId);
            SetBusHeader(message, MessagingConstants.HeaderNames.SentTime, busMessage.Sent);
        }

        public object Deserialize(DataContractKey contractKey, Type dataType, byte[] body)
        {
            DataContractSerializer serializer = new DataContractSerializer(dataType);
            
            using (MemoryStream memoryStream = new MemoryStream(body))
            {
                using (Message message = _encoderFactory.Encoder.ReadMessage(memoryStream, int.MaxValue))
                {
                    using (XmlDictionaryReader bodyContents = message.GetReaderAtBodyContents())
                    {
                        return serializer.ReadObject(bodyContents);
                    }
                }   
            }
        }
    }
}