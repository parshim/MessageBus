using System;

namespace MessageBus.Core.API
{
    public interface IPublisher : IDisposable
    {
        void Send<TData>(TData data);
    }
}