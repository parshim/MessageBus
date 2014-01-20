namespace MessageBus.Core.API
{
    internal interface ICallHandler
    {
        void Dispatch(RawBusMessage message);
    }
}