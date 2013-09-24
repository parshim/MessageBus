using System;

namespace MessageBus.Core.API
{
    public interface IBus : IDisposable
    {
        IPublisher CreatePublisher();

        bool Register<TData>(Action<TData> callback);
    }
}
