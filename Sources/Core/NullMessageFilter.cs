using System.Collections.Generic;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class NullMessageFilter : IMessageFilter
    {
        public void ApplyFilters(IEnumerable<MessageFilterInfo> filters)
        {
            
        }
    }
}