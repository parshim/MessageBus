using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Subscriber : ISubscriber
    {
        private readonly IInputChannel _inputChannel;
        private readonly Thread _receiver;
        private bool _receive;

        private readonly string _busId;

        private readonly IErrorSubscriber _errorSubscriber;

        private class MessageInfo
        {
            private readonly IDispatcher _dispatcher;
            private readonly XmlObjectSerializer _serializer;
            private readonly bool _receiveSelfPublish;

            public MessageInfo(IDispatcher dispatcher, XmlObjectSerializer serializer, bool receiveSelfPublish)
            {
                _dispatcher = dispatcher;
                _serializer = serializer;
                _receiveSelfPublish = receiveSelfPublish;
            }

            public IDispatcher Dispatcher
            {
                get { return _dispatcher; }
            }

            public XmlObjectSerializer Serializer
            {
                get { return _serializer; }
            }

            public bool ReceiveSelfPublish
            {
                get { return _receiveSelfPublish; }
            }
        }

        private readonly ConcurrentDictionary<DataContractKey, MessageInfo> _registeredTypes = new ConcurrentDictionary<DataContractKey, MessageInfo>();

        public Subscriber(IInputChannel inputChannel, string busId, IErrorSubscriber errorSubscriber)
        {
            _busId = busId;
            _errorSubscriber = errorSubscriber;

            _inputChannel = inputChannel;
            
            _inputChannel.Open();

            _receive = true;

            _receiver = new Thread(MessagePump);
            _receiver.Start();
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
                        RawBusMessage busMessage = ReadMessage(message);

                        MessageInfo messageInfo;

                        if (!_registeredTypes.TryGetValue(new DataContractKey(busMessage.Name, busMessage.Namespace), out messageInfo))
                        {
                            _errorSubscriber.UnregisteredMessageArrived(busMessage);

                            continue;
                        }

                        if (!IsMessageSurvivesFilter(messageInfo, busMessage))
                        {
                            _errorSubscriber.MessageFilteredOut(busMessage);

                            continue;
                        }

                        try
                        {
                            messageInfo.Dispatcher.Dispatch(busMessage);
                        }
                        catch(Exception ex)
                        {
                            _errorSubscriber.MessageDispatchException(busMessage, ex);
                        }
                    }
                }
            }
        }

        private bool IsMessageSurvivesFilter(MessageInfo messageInfo, RawBusMessage busMessage)
        {
            if (messageInfo.ReceiveSelfPublish) return true;

            bool selfPublished = Equals(busMessage.BusId, _busId);

            return !selfPublished;
        }

        private RawBusMessage ReadMessage(Message message)
        {
            string busId = message.Headers.GetHeader<string>(MessagingConstancts.HeaderNames.BusId,
                                                             MessagingConstancts.Namespace.MessageBus,
                                                             MessagingConstancts.Actor.Bus);

            DateTime sent = message.Headers.GetHeader<DateTime>(MessagingConstancts.HeaderNames.SentTime,
                                                             MessagingConstancts.Namespace.MessageBus,
                                                             MessagingConstancts.Actor.Bus);

            RawBusMessage rawBusMessage = new RawBusMessage
                {
                    BusId = busId,
                    Sent = sent
                };

            foreach (MessageHeaderInfo headerInfo in message.Headers.Where(info => info.Actor == MessagingConstancts.Actor.User &&
                                                                                    info.Namespace == MessagingConstancts.Namespace.MessageBus))
            {
                string header = message.Headers.GetHeader<string>(headerInfo.Name, headerInfo.Namespace, headerInfo.Actor);

                rawBusMessage.Headers.Add(headerInfo.Name, header);
            }

            using (XmlDictionaryReader bodyContents = message.GetReaderAtBodyContents())
            {
                rawBusMessage.Name = bodyContents.Name;
                rawBusMessage.Namespace = bodyContents.NamespaceURI;

                MessageInfo messageInfo;
                if (_registeredTypes.TryGetValue(new DataContractKey(rawBusMessage.Name, rawBusMessage.Namespace), out messageInfo))
                {
                    try
                    {
                        rawBusMessage.Data = messageInfo.Serializer.ReadObject(bodyContents);
                    }
                    catch (Exception ex)
                    {
                        _errorSubscriber.MessageDeserializeException(rawBusMessage, ex);
                    }
                }
            }

            return rawBusMessage;
        }

        public bool Subscribe<TData>(Action<TData> callback, bool hierarchy, bool receiveSelfPublish)
        {
            return Subscribe(typeof (TData), o => callback((TData) o), hierarchy, receiveSelfPublish);
        }
        
        public bool Subscribe(Type dataType, Action<object> callback, bool hierarchy, bool receiveSelfPublish)
        {
            ActionDispatcher actionDispatcher = new ActionDispatcher(callback);
           
            return Subscribe(dataType, actionDispatcher, hierarchy, receiveSelfPublish);
        }

        public bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy, bool receiveSelfPublish)
        {
            return Subscribe(typeof(TData), new BusMessageDispatcher<TData>(callback), hierarchy, receiveSelfPublish);
        }

        public bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy, bool receiveSelfPublish)
        {
            return Subscribe(dataType, new RawDispatcher(callback), hierarchy, receiveSelfPublish);
        }

        private bool Subscribe(Type dataType, IDispatcher dispatcher, bool hierarchy, bool receiveSelfPublish)
        {
            if (hierarchy)
            {
                return SubscribeHierarchy(dataType, dispatcher, receiveSelfPublish);
            }

            return Subscribe(dataType, dispatcher, receiveSelfPublish);
        }

        private bool Subscribe(Type dataType, IDispatcher dispatcher, bool receiveSelfPublish)
        {
            DataContract dataContract = new DataContract(dataType);

            return _registeredTypes.TryAdd(dataContract.Key, new MessageInfo(dispatcher, dataContract.Serializer, receiveSelfPublish));
        }

        private bool SubscribeHierarchy(Type baseType, IDispatcher dispatcher, bool receiveSelfPublish)
        {
            var types = from type in baseType.Assembly.GetTypes()
                        where type != baseType && baseType.IsAssignableFrom(type)
                        select type;

            bool atLeastOne = false;

            foreach (Type type in types)
            {
                atLeastOne = Subscribe(type, dispatcher, receiveSelfPublish) || atLeastOne;
            }

            return atLeastOne;
        }

        public void Dispose()
        {
            _receive = false;

            _receiver.Join(TimeSpan.FromMilliseconds(200));

            _inputChannel.Close();
        }
    }
}