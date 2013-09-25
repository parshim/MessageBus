using System.ServiceModel.Channels;

namespace MessageBus.Core.API
{
    public interface IContractHandler
    {
        IProcessor CreateProcessor(MessageBuffer messageBuffer);
    }
}