using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Publisher : IPublisher
    {
        private readonly IOutputChannel _outputChannel;
        private readonly MessageVersion _messageVersion;
        private readonly string _busId;

        public Publisher(IOutputChannel outputChannel, MessageVersion messageVersion, string busId)
        {
            _outputChannel = outputChannel;
            _messageVersion = messageVersion;
            _busId = busId;

            _outputChannel.Open();
        }

        public void Send<TData>(TData data)
        {
            Send(new BusMessage<TData> { Data = data });
        }

        public void Send<TData>(BusMessage<TData> data)
        {
            using (Message message = Message.CreateMessage(_messageVersion, MessagingConstancts.MessageAction.Regular, data.Data))
            {
                message.Headers.Add(MessageHeader.CreateHeader(MessagingConstancts.HeaderNames.BusId,
                                                               MessagingConstancts.Namespace.MessageBus, _busId, false,
                                                               MessagingConstancts.Actor.Bus));
                
                message.Headers.Add(MessageHeader.CreateHeader(MessagingConstancts.HeaderNames.SentTime,
                                                               MessagingConstancts.Namespace.MessageBus, DateTime.Now, false,
                                                               MessagingConstancts.Actor.Bus));

                foreach (KeyValuePair<string, string> pair in data.Headers)
                {
                    message.Headers.Add(MessageHeader.CreateHeader(pair.Key, MessagingConstancts.Namespace.MessageBus, pair.Value, false, MessagingConstancts.Actor.User));
                }

                _outputChannel.Send(message);
            }
        }

        public void Dispose()
        {
            _outputChannel.Close();
        }
    }
}