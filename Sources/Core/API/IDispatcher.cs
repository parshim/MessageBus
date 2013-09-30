namespace MessageBus.Core.API
{
    internal interface IDispatcher
    {
        void Dispatch(RawBusMessage message);
    }
}