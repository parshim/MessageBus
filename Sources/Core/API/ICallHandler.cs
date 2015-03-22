namespace MessageBus.Core.API
{
    public interface ICallHandler
    {
        void Dispatch(RawBusMessage message);
    }
}