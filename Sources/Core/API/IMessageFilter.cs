using System.Collections.Generic;

namespace MessageBus.Core.API
{
    internal interface IMessageFilter
    {
        void ApplyFilters(IEnumerable<MessageFilterInfo> filters);
    }
}
