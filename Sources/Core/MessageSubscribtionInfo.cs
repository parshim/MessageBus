using System.Collections.Generic;
using System.Runtime.Serialization;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class MessageSubscribtionInfo
    {
        private readonly ICallHandler _handler;
        private readonly XmlObjectSerializer _serializer;
        private readonly MessageFilterInfo _filterInfo;

        public MessageSubscribtionInfo(DataContractKey contractKey, ICallHandler handler, XmlObjectSerializer serializer, bool receiveSelfPublish, IEnumerable<BusHeader> filterHeaders)
        {
            _handler = handler;
            _serializer = serializer;

            _filterInfo = new MessageFilterInfo(contractKey, receiveSelfPublish, filterHeaders);
        }

        public ICallHandler Handler
        {
            get { return _handler; }
        }

        public XmlObjectSerializer Serializer
        {
            get { return _serializer; }
        }

        public MessageFilterInfo FilterInfo
        {
            get { return _filterInfo; }
        }
    }
}