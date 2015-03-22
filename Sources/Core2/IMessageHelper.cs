using MessageBus.Core;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public interface IMessageHelper
    {
        RawBusMessage ConstructMessage(DataContractKey dataContractKey, IBasicProperties properties, object data);

        BusMessage<T> ConstructMessage<T>(DataContractKey dataContractKey, IBasicProperties properties, T data);
    }
}