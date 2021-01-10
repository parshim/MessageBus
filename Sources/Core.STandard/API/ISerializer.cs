using System;

namespace MessageBus.Core.API
{
    public interface ISerializer
    {
        string ContentType { get; }

        byte[] Serialize(RawBusMessage data);

        object Deserialize(Type dataType, byte[] body);
    }
}