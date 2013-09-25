using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Publisher : IPublisher
    {
        private readonly IOutputChannel _outputChannel;
        private readonly MessageVersion _messageVersion;

        public Publisher(IOutputChannel outputChannel, MessageVersion messageVersion)
        {
            _outputChannel = outputChannel;
            _messageVersion = messageVersion;

            _outputChannel.Open();
        }

        public void Send<TData>(TData data)
        {
            using (Message message = Message.CreateMessage(_messageVersion, "Message", data))
            {
                _outputChannel.Send(message);
            }
        }

        public void Dispose()
        {
            _outputChannel.Close();
        }
    }
}