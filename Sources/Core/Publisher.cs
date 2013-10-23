using System;
using System.Collections.Concurrent;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class Publisher : IPublisher
    {
        private readonly ConcurrentDictionary<Type, DataContractKey> _nameMappings = new ConcurrentDictionary<Type, DataContractKey>();
        private readonly IOutputChannel _outputChannel;
        private readonly MessageVersion _messageVersion;
        private readonly FaultMessageProcessor _faultMessageProcessor;
        private readonly string _busId;

        public Publisher(IOutputChannel outputChannel, MessageVersion messageVersion, FaultMessageProcessor faultMessageProcessor, string busId)
        {
            _outputChannel = outputChannel;
            _messageVersion = messageVersion;
            _faultMessageProcessor = faultMessageProcessor;
            _busId = busId;

            _outputChannel.Open();
        }

        public void Send<TData>(TData data)
        {
            Send(new BusMessage<TData> { Data = data });
        }

        public void Send<TData>(BusMessage<TData> busMessage)
        {
            DataContractKey contractKey;
            Type type = typeof (TData);

            if (!_nameMappings.TryGetValue(type, out contractKey))
            {
                DataContract contract = new DataContract(busMessage.Data);

                _nameMappings.TryAdd(type, contract.Key);

                _faultMessageProcessor.AddKnownContract(contract);

                contractKey = contract.Key;
            }

            using (Message message = Message.CreateMessage(_messageVersion, MessagingConstancts.MessageAction.Regular, busMessage.Data))
            {
                SetBusHeaders(message, contractKey);

                SetUserHeaders(busMessage, message);

                _outputChannel.Send(message);
            }
        }

        private static void SetUserHeaders<TData>(BusMessage<TData> busMessage, Message message)
        {
            foreach (BusHeader busHeader in busMessage.Headers)
            {
                message.Headers.Add(MessageHeader.CreateHeader(busHeader.Name,
                                                               MessagingConstancts.Namespace.MessageBus,
                                                               busHeader.Value, false,
                                                               MessagingConstancts.Actor.User));
            }
        }

        private void SetBusHeaders(Message message, DataContractKey contractKey)
        {
            SetBusHeader(message, MessagingConstancts.HeaderNames.Name, contractKey.Name);
            SetBusHeader(message, MessagingConstancts.HeaderNames.NameSpace, contractKey.Ns);
            SetBusHeader(message, MessagingConstancts.HeaderNames.BusId, _busId);
            SetBusHeader(message, MessagingConstancts.HeaderNames.SentTime, DateTime.Now);
        }

        private void SetBusHeader(Message message, string name, object value)
        {
            message.Headers.Add(MessageHeader.CreateHeader(name, MessagingConstancts.Namespace.MessageBus,
                                                           value, false, MessagingConstancts.Actor.Bus));
        }

        public virtual void Dispose()
        {
            _outputChannel.Close();
        }
    }
}