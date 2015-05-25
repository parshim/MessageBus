using System;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public interface IChannelFactory<out T> : IDisposable where T : class
    {
        T CreateChannel(params BusHeader[] headers);

        T CreateChannel(IHeadersProvider headersProvider);
    }
}