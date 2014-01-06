using System.ServiceModel.Channels;
using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class FaultMessageProcessor : IFaultMessageProcessor
    {
        private readonly IPublishingErrorHandler _errorHandler;
        private readonly IKnownContractCollector _collector;

        private readonly RawBusMessageReader _reader = new RawBusMessageReader();

        public FaultMessageProcessor(IPublishingErrorHandler errorHandler, IKnownContractCollector collector)
        {
            _errorHandler = errorHandler;
            _collector = collector;
        }

        public void Process(int code, string text, Message message)
        {
            RawBusMessage busMessage = _reader.ReadMessage(message, _collector.Deserialize);

            _errorHandler.DeliveryFailed(code, text, busMessage);
        }
    }
}