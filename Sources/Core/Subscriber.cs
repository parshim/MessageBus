using System;
using System.ServiceModel.Channels;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal sealed class Subscriber : SubscriberBase
    {
        internal Subscriber(IInputChannel inputChannel, string busId, IErrorSubscriber errorSubscriber, IMessageFilter messageFilter)
            : base(inputChannel, busId, errorSubscriber, messageFilter)
        {
        }

        protected override void MessagePump()
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
    }
}