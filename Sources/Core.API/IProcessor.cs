namespace MessageBus.Core.API
{
    public interface IProcessor<in TData>
    {
        void Process(TData data);
    }
}