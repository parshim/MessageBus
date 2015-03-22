using MessageBus.Core.API;

namespace MessageBus.Core
{
    public interface ICallHandler
    {
        void Dispatch(RawBusMessage message);
    }
}