using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Xml;

using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class FaultMessageProcessor : IFaultMessageProcessor
    {
        private readonly IPublishingErrorHandler _errorHandler;
        private readonly RawBusMessageReader _reader = new RawBusMessageReader();
        private readonly ConcurrentDictionary<DataContractKey, XmlObjectSerializer> _map = new ConcurrentDictionary<DataContractKey, XmlObjectSerializer>();

        public FaultMessageProcessor(IPublishingErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public void Process(int code, string text, Message message)
        {
            RawBusMessage busMessage = _reader.ReadMessage(message, Deserializer);

            _errorHandler.DeliveryFailed(code, text, busMessage);
        }

        private void Deserializer(RawBusMessage rawBusMessage, XmlDictionaryReader bodyContent)
        {
            XmlObjectSerializer serializer;
            if (_map.TryGetValue(new DataContractKey(rawBusMessage.Name, rawBusMessage.Namespace), out serializer))
            {
                rawBusMessage.Data = serializer.ReadObject(bodyContent);
            }
        }

        public void AddKnownContract(DataContract contract)
        {
            _map.TryAdd(contract.Key, contract.Serializer);
        }
    }
}