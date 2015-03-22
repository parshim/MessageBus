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
        private readonly IKnownContractCollector _contractCollector;
        private readonly MessageVersion _messageVersion;
        
        private readonly string _busId;

        public Publisher(IOutputChannel outputChannel, MessageVersion messageVersion, IKnownContractCollector contractCollector, string busId)
        {
            _outputChannel = outputChannel;
            _messageVersion = messageVersion;
            _busId = busId;
            _contractCollector = contractCollector;

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

                _contractCollector.AddKnownContract(contract);

                contractKey = contract.Key;
            }

            using (Message message = Message.CreateMessage(_messageVersion, MessagingConstants.MessageAction.Regular, busMessage.Data))
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
                                                               MessagingConstants.Namespace.MessageBus,
                                                               busHeader.Value, false,
                                                               MessagingConstants.Actor.User));
            }
        }

        private void SetBusHeaders(Message message, DataContractKey contractKey)
        {
            SetBusHeader(message, MessagingConstants.HeaderNames.Name, contractKey.Name);
            SetBusHeader(message, MessagingConstants.HeaderNames.NameSpace, contractKey.Ns);
            SetBusHeader(message, MessagingConstants.HeaderNames.BusId, _busId);
            SetBusHeader(message, MessagingConstants.HeaderNames.SentTime, DateTime.Now);
        }

        private void SetBusHeader(Message message, string name, object value)
        {
            message.Headers.Add(MessageHeader.CreateHeader(name, MessagingConstants.Namespace.MessageBus,
                                                           value, false, MessagingConstants.Actor.Bus));
        }

        public virtual void Dispose()
        {
            _outputChannel.Close();
        }
    }
}