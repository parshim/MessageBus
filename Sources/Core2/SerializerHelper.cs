using System;
using System.Text;
using Newtonsoft.Json;

namespace MessageBus.Core
{
    public class SerializerHelper : ISerializerHelper
    {
        private readonly Encoding _encoding = Encoding.Unicode;

        public byte[] Serialize<TData>(TData data)
        {
            if (typeof(TData) == typeof(byte[]))
            {
                return data as byte[];
            }
            
            string body = JsonConvert.SerializeObject(data, Formatting.None);

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