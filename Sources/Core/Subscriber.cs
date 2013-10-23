using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public abstract class Subscriber : ISubscriber
    {
        private readonly IInputChannel _inputChannel;
        private readonly Thread _receiver;
        private bool _receive;
        private bool _stared;

        private readonly string _busId;

        private readonly RawBusMessageReader _reader = new RawBusMessageReader();

        private readonly IErrorSubscriber _errorSubscriber;
        private readonly ConcurrentDictionary<DataContractKey, MessageSubscribtionInfo> _registeredTypes = new ConcurrentDictionary<DataContractKey, MessageSubscribtionInfo>();

        protected Subscriber(IInputChannel inputChannel, string busId, IErrorSubscriber errorSubscriber)
        {
            _busId = busId;
            _errorSubscriber = errorSubscriber;
            _inputChannel = inputChannel;
            _receiver = new Thread(MessagePump);
        }

        private void MessagePump()
        {
            while (_receive)
            {
                Message message;

                if (_inputChannel.TryReceive(TimeSpan.FromMilliseconds(100), out message))
                {
                    using (message)
                    {
                        MessageSubscribtionInfo messageSubscribtionInfo;

                        Action<RawBusMessage, XmlDictionaryReader> provider = (msg, reader) =>
                            {
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

                        RawBusMessage busMessage = _reader.ReadMessage(message, provider);

                        
                        
                        if (!_registeredTypes.TryGetValue(new DataContractKey(busMessage.Name, busMessage.Namespace), out messageSubscribtionInfo))
                        {
                            _errorSubscriber.UnregisteredMessageArrived(busMessage);

                            continue;
                        }

                        if (!IsMessageSurvivesFilter(messageSubscribtionInfo.FilterInfo, busMessage))
                        {
                            _errorSubscriber.MessageFilteredOut(busMessage);

                            continue;
                        }

                        try
                        {
                            messageSubscribtionInfo.Dispatcher.Dispatch(busMessage);
                        }
                        catch(Exception ex)
                        {
                            _errorSubscriber.MessageDispatchException(busMessage, ex);
                        }
                    }
                }
            }
        }

        private bool IsMessageSurvivesFilter(MessageFilterInfo filterInfo, RawBusMessage busMessage)
        {
            // TODO: Add header filtering

            if (filterInfo.ReceiveSelfPublish) return true;

            bool selfPublished = Equals(busMessage.BusId, _busId);

            return !selfPublished;
        }

        public bool Subscribe<TData>(Action<TData> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            return Subscribe(typeof (TData), o => callback((TData) o), hierarchy, receiveSelfPublish, filter);
        }

        public bool Subscribe(Type dataType, Action<object> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            ActionDispatcher actionDispatcher = new ActionDispatcher(callback);
           
            return Subscribe(dataType, actionDispatcher, hierarchy, receiveSelfPublish, filter);
        }

        public bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            return Subscribe(typeof(TData), new BusMessageDispatcher<TData>(callback), hierarchy, receiveSelfPublish, filter);
        }

        public bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            return Subscribe(dataType, new RawDispatcher(callback), hierarchy, receiveSelfPublish, filter);
        }

        public void StartProcessMessages()
        {
            if (_stared) return;
            
            _inputChannel.Open();

            _receive = true;
            _stared = true;

            ApplyFilters(_registeredTypes.Values.Select(info => info.FilterInfo));

            _receiver.Start();
        }

        protected abstract void ApplyFilters(IEnumerable<MessageFilterInfo> filters);

        private bool Subscribe(Type dataType, IDispatcher dispatcher, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            if (hierarchy)
            {
                return SubscribeHierarchy(dataType, dispatcher, receiveSelfPublish, filter);
            }

            return Subscribe(dataType, dispatcher, receiveSelfPublish, filter);
        }

        private bool Subscribe(Type dataType, IDispatcher dispatcher, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            if (_stared) throw new SubscribtionClosedException();

            DataContract dataContract = new DataContract(dataType);

            return _registeredTypes.TryAdd(dataContract.Key,
                                           new MessageSubscribtionInfo(dataContract.Key, dispatcher,
                                                                       dataContract.Serializer, receiveSelfPublish,
                                                                       filter ?? Enumerable.Empty<BusHeader>()));
        }

        private bool SubscribeHierarchy(Type baseType, IDispatcher dispatcher, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            var types = from type in baseType.Assembly.GetTypes()
                        where type != baseType && baseType.IsAssignableFrom(type)
                        select type;

            bool atLeastOne = false;

            foreach (Type type in types)
            {
                atLeastOne = Subscribe(type, dispatcher, receiveSelfPublish, filter) || atLeastOne;
            }

            return atLeastOne;
        }

        public void Dispose()
        {
            _receive = false;

            if (_stared)
            {
                _receiver.Join(TimeSpan.FromMilliseconds(200));

                _inputChannel.Close();
            }
        }
    }
}