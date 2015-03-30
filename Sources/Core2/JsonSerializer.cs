using System;
using System.Text;
using MessageBus.Core.API;
using Newtonsoft.Json;

namespace MessageBus.Core
{
    public class JsonSerializer : ISerializer
    {
        private readonly Encoding _encoding = Encoding.Unicode;

        public string ContentType { get { return "application/json"; } }

        public byte[] Serialize<TData>(DataContractKey contractKey, BusMessage<TData> busMessage)
        {
            if (typeof(TData) == typeof(byte[]))
            {
                return busMessage.Data as byte[];
            }

            string body = JsonConvert.SerializeObject(busMessage.Data, Formatting.None);

            return _encoding.GetBytes(body);
        }

        public object Deserialize(DataContractKey dataContractKey, Type dataType, byte[] body)
        {
            if (dataContractKey.Equals(DataContractKey.BinaryBlob))
            {
                return body;
            }

            string sBody = _encoding.GetString(body);

            return JsonConvert.DeserializeObject(sBody, dataType);
        }
    }
}