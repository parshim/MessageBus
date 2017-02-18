using System;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;

using MessageBus.Core.API;
using System.Xml;

namespace MessageBus.Core
{
    public class XmlSerializer : ISerializer
    {
        public string ContentType { get { return "application/xml"; } }

        public byte[] Serialize(RawBusMessage busMessage)
        {
            object data = busMessage.Data;

            DataContractSerializer serializer = new DataContractSerializer(data.GetType());

            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    serializer.WriteObject(writer, busMessage.Data);

                    writer.Flush();

                    return stream.ToArray();
                }
            }
        }

        public object Deserialize(DataContractKey contractKey, Type dataType, byte[] body)
        {
            DataContractSerializer serializer = new DataContractSerializer(dataType);

            using (MemoryStream stream = new MemoryStream(body, false))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    return serializer.ReadObject(reader);
                }
            }   
        }
    }
}
