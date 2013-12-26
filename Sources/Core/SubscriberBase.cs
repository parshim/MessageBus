using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal abstract class SubscriberBase : ISubscriber
    {
        protected IInputChannel _inputChannel;
        private readonly Thread _receiver;
        protected bool _receive;
        private bool _stared;
        private readonly string _busId;
        protected readonly RawBusMessageReader _reader = new RawBusMessageReader();
        protected IErrorSubscriber _errorSubscriber;
        protected readonly ConcurrentDictionary<DataContractKey, MessageSubscribtionInfo> _registeredTypes = new ConcurrentDictionary<DataContractKey, MessageSubscribtionInfo>();
        private readonly IMessageFilter _messageFilter;

        protected SubscriberBase(IInputChannel inputChannel, string busId, IErrorSubscriber errorSubscriber, IMessageFilter messageFilter)
        {
            _busId = busId;
            _errorSubscriber = errorSubscriber;
            _messageFilter = messageFilter;
            _inputChannel = inputChannel;
            _receiver = new Thread(MessagePump);
        }

        protected abstract void MessagePump();

        protected bool IsMessageSurvivesFilter(MessageFilterInfo filterInfo, RawBusMessage busMessage)
        {
            // TODO: Add header filtering

            if (filterInfo.ReceiveSelfPublish) return true;

            bool selfPublished = Equals(busMessage.BusId, _busId);

            return !selfPublished;
        }

        public bool Subscribe<TData>(Action<TData> callback, bool hierarchy, bool receiveSelfPublish, IEnumerable<BusHeader> filter)
        {
            return Subscribe(typeof(TData), o => callback((TData)o), hierarchy, receiveSelfPublish, filter);
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

        protected void ApplyFilters(IEnumerable<MessageFilterInfo> filters)
        {
            _messageFilter.ApplyFilters(filters);
        }

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