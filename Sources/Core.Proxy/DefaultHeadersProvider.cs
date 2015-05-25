using System.Collections.Generic;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public class DefaultHeadersProvider : IHeadersProvider
    {
        private readonly BusHeader[] _headers;

        public DefaultHeadersProvider(BusHeader[] headers)
        {
            _headers = headers;
        }

        public IEnumerable<BusHeader> GetMessageHeaders()
        {
            return _headers;
        }
    }
}