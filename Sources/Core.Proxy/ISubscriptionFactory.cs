namespace MessageBus.Core.Proxy
{
    public interface ISubscriptionFactory<T> where T : class
    {
        ISubscriptionSelector<T> Subscribe();
    }
}