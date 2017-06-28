using System;
using System.IO;
using Google.Protobuf;
using MessageBus.Core.API;

namespace MessageBus.Core.Protobuf
{
    public class ProtobufSerializer : ISerializer
    {
        public byte[] Serialize(RawBusMessage data)
        {
            IMessage message = (IMessage)data.Data;

            using (MemoryStream stream = new MemoryStream())
            {
                message.WriteTo(stream);

                return stream.ToArray();
            }
        }

        public object Deserialize(Type dataType, byte[] body)
        {
            IMessage message = (IMessage) Activator.CreateInstance(dataType);

            message.MergeFrom(body);

            return message;
        }

        public string ContentType { get; } = "application/protobuf";
    }

}
