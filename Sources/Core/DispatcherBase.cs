using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class DispatcherBase : IDispatcher
    {
        protected readonly ConcurrentDictionary<DataContractKey, MessageSubscribtionInfo> _registeredTypes = new ConcurrentDictionary<DataContractKey, MessageSubscribtionInfo>();
        
        private readonly RawBusMessageReader _reader = new RawBusMessageReader();
        private readonly IErrorSubscriber _errorSubscriber;
        private readonly string _busId;

        public DispatcherBase(IErrorSubscriber errorSubscriber, string busId)
        {
            _errorSubscriber = errorSubscriber;
            _busId = busId;
        }

        public IEnumerable<MessageFilterInfo> GetApplicableFilters()
        {
            return _registeredTypes.Values.Select(info => info.FilterInfo);
        }

        private RawBusMessage ReadMessage(Message message)
        {
            Action<RawBusMessage, XmlDictionaryReader> provider = (msg, reader) =>
                {
                    MessageSubscribtionInfo messageSubscribtionInfo;

                    if (!_registeredTypes.TryGetValue(new DataContractKey(msg.Name, msg.Namespace), out messageSubscribtionInfo))
                    {
                        return;
                    }

                    try
                    {
                        msg.Data = messageSubscribtionInfo.Serializer.ReadObject(reader);
                    }
                    catch (Exception ex)
                    {
                        _errorSubscriber.MessageDeserializeException(msg, ex);
                    }
                };

            return _reader.ReadMessage(message, provider);
        }

        public void Dispatch(Message message)
        {
            RawBusMessage busMessage = ReadMessage(message);

            MessageSubscribtionInfo messageSubscribtionInfo;

            if (!_registeredTypes.TryGetValue(new DataContractKey(busMessage.Name, busMessage.Namespace), out messageSubscribtionInfo))
            {
                _errorSubscriber.UnregisteredMessageArrived(busMessage);

                return;
            }

            if (!IsMessageSurvivesFilter(messageSubscribtionInfo.FilterInfo, busMessage))
            {
                _errorSubscriber.MessageFilteredOut(busMessage);

                return;
            }

            try
            {
                messageSubscribtionInfo.Handler.Dispatch(busMessage);
            }
            catch (Exception ex)
            {
                _errorSubscriber.MessageDispatchException(busMessage, ex);
            }
        }

        private bool IsMessageSurvivesFilter(MessageFilterInfo filterInfo, RawBusMessage busMessage)
        {
            // TODO: Add header filtering

            if (filterInfo.ReceiveSelfPublish) return true;

            bool selfPublished = Equals(busMessage.BusId, _busId);

            return !selfPublished;
        }
    }
}