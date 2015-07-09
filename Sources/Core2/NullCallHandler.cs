using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class NullCallHandler : ICallHandler
    {
        public RawBusMessage Dispatch(RawBusMessage message)
        {
            return new RawBusMessage();
        }
    }
}