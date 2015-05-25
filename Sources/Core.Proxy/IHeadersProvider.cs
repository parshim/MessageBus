using System.Collections.Generic;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public interface IHeadersProvider
    {
        IEnumerable<BusHeader> GetMessageHeaders();
    }
}