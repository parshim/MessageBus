using System.ServiceModel.Channels;

namespace MessageBus.Core.API
{
    public interface IReceiver
    {
        IProcessor CreateProcessor(MessageBuffer messageBuffer);
    }
}