using System.Collections.Generic;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class MessageFilterInfo
    {
        private readonly DataContractKey _contractKey;
        private readonly bool _receiveSelfPublish;
        private readonly List<BusHeader> _filterHeaders;

        public MessageFilterInfo(DataContractKey contractKey, bool receiveSelfPublish, IEnumerable<BusHeader> filterHeaders)
        {
            _contractKey = contractKey;
            _receiveSelfPublish = receiveSelfPublish;
            _filterHeaders = new List<BusHeader>(filterHeaders);
        }

        public DataContractKey ContractKey
        {
            get { return _contractKey; }
        }

        public bool ReceiveSelfPublish
        {
            get { return _receiveSelfPublish; }
        }

        public IEnumerable<BusHeader> FilterHeaders
        {
            get { return _filterHeaders; }
        }
    }
}