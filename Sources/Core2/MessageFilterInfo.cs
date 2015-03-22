using System.Collections.Generic;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class MessageFilterInfo
    {
        private readonly DataContractKey _contractKey;
        private readonly List<BusHeader> _filterHeaders;

        public MessageFilterInfo(DataContractKey contractKey, IEnumerable<BusHeader> filterHeaders)
        {
            _contractKey = contractKey;
            _filterHeaders = new List<BusHeader>(filterHeaders);
        }

        public DataContractKey ContractKey
        {
            get { return _contractKey; }
        }
        
        public IEnumerable<BusHeader> FilterHeaders
        {
            get { return _filterHeaders; }
        }
    }
}