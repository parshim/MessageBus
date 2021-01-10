using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class NullTrace : ITrace
    {
        public void MessageArrived(string busId, RawBusMessage busMessage, string consumerTag)
        {
        }

        public void MessageSent(string busId, RawBusMessage busMessage)
        {
        }
    }
}