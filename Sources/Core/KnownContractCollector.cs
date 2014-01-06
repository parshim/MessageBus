using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class KnownContractCollector : IKnownContractCollector
    {
        private readonly ConcurrentDictionary<DataContractKey, XmlObjectSerializer> _map = new ConcurrentDictionary<DataContractKey, XmlObjectSerializer>();

        public void AddKnownContract(DataContract contract)
        {
            _map.TryAdd(contract.Key, contract.Serializer);
        }

        public void Deserialize(RawBusMessage rawBusMessage, XmlDictionaryReader bodyContent)
        {
            XmlObjectSerializer serializer;
            if (_map.TryGetValue(new DataContractKey(rawBusMessage.Name, rawBusMessage.Namespace), out serializer))
            {
                rawBusMessage.Data = serializer.ReadObject(bodyContent);
            }
        }
    }
}