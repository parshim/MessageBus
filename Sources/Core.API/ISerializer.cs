using System;

namespace MessageBus.Core.API
{
    public interface ISerializer
    {
        string ContentType { get; }

        byte[] Serialize<TData>(DataContractKey contractKey, BusMessage<TData> data);

        object Deserialize(DataContractKey dataContractKey, Type dataType, byte[] body);
    }
}