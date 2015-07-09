using MessageBus.Core.API;

namespace MessageBus.Core
{
    public interface ICallHandler
    {
        RawBusMessage Dispatch(RawBusMessage message);
    }

}