using System.Collections.Generic;
using System.Runtime.Serialization;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class MessageSubscribtionInfo
    {
        private readonly IDispatcher _dispatcher;
        private readonly XmlObjectSerializer _serializer;
        private readonly MessageFilterInfo _filterInfo;

        public MessageSubscribtionInfo(DataContractKey contractKey, IDispatcher dispatcher, XmlObjectSerializer serializer, bool receiveSelfPublish, IEnumerable<BusHeader> filterHeaders)
        {
            _dispatcher = dispatcher;
            _serializer = serializer;

            _filterInfo = new MessageFilterInfo(contractKey, receiveSelfPublish, filterHeaders);
        }

        public IDispatcher Dispatcher
        {
            get { return _dispatcher; }
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