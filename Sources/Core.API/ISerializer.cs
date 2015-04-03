using System;

namespace MessageBus.Core.API
{
    public interface ISerializer
    {
        string ContentType { get; }

        byte[] Serialize(RawBusMessage data);

        object Deserialize(DataContractKey dataContractKey, Type dataType, byte[] body);
    }
}