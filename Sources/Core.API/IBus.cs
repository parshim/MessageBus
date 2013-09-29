using System;
using Microsoft.Practices.ServiceLocation;

namespace MessageBus.Core.API
{
    public interface IBus : IDisposable
    {
        IPublisher CreatePublisher();

        ISubscriber CreateSubscriber();
        
        IAutoLocatingSubscriber CreateSubscriber(IServiceLocator serviceLocator);
    }
}
