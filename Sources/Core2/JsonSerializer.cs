using System;
using System.Text;

using MessageBus.Core.API;
using Newtonsoft.Json;

namespace MessageBus.Core
{
    public class JsonSerializer : ISerializer
    {
        private readonly Encoding _encoding = Encoding.UTF8;

        private readonly JsonSerializerSettings _settings;

        public JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        public string ContentType { get { return "application/json"; } }
        
        public byte[] Serialize(RawBusMessage busMessage)
        {
            object data = busMessage.Data;

            if (data.GetType() == typeof(byte[]))
            {
                return data as byte[];
            }

            string body = JsonConvert.SerializeObject(data, _settings);

            return _encoding.GetBytes(body);
        }

        public object Deserialize(Type dataType, byte[] body)
        {
            string sBody = _encoding.GetString(body);

            return JsonConvert.DeserializeObject(sBody, dataType, _settings);
        }
    }
}