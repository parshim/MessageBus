using System;
using System.IO;
using System.Text;
using MessageBus.Core.API;
using System.Xml;
using System.Xml.Serialization;

namespace MessageBus.Core
{
    public class XmlSerializer : ISerializer
    {
        public string ContentType { get { return "application/xml"; } }

        public byte[] Serialize(RawBusMessage busMessage)
        {
            object data = busMessage.Data;

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(data.GetType());

            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");

            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = false,
                    NewLineHandling = NewLineHandling.None,
                    
                }))
                {
                    serializer.Serialize(writer, busMessage.Data, ns);

                    writer.Flush();

                    return stream.ToArray();
                }
            }
        }

        public object Deserialize(Type dataType, byte[] body)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(dataType);

            using (MemoryStream stream = new MemoryStream(body, false))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    return serializer.Deserialize(reader);
                }
            }   
        }
    }
}
