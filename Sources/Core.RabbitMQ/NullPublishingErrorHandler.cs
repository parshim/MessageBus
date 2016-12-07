using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class NullPublishingErrorHandler : IPublishingErrorHandler
    {
        public void DeliveryFailed(int errorCode, string text, RawBusMessage message)
        {
            
        }
    }
}