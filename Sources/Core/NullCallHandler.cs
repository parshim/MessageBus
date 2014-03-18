using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class NullCallHandler : ICallHandler
    {
        public void Dispatch(RawBusMessage message)
        {
            
        }
    }
}