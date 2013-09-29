namespace MessageBus.Core.API
{
    public interface IAutoLocatingSubscriber : ISubscriber
    {
        bool Subscribe<TData>();
    }
}