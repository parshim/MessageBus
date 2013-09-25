using System;

namespace MessageBus.Core.API
{
    public interface IBus : IDisposable
    {
        IPublisher CreatePublisher();

        ISubscriber CreateSubscriber();
    }
}
