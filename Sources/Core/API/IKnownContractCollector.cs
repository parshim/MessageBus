using System.Xml;

namespace MessageBus.Core.API
{
    internal interface IKnownContractCollector
    {
        void AddKnownContract(DataContract contract);
        void Deserialize(RawBusMessage rawBusMessage, XmlDictionaryReader bodyContent);
    }
}