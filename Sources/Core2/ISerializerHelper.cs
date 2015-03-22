using System;
using MessageBus.Core;

namespace MessageBus.Core
{
    public interface ISerializerHelper
    {
        byte[] Serialize<TData>(TData data);

        object Deserialize(DataContractKey dataContractKey, Type dataType, byte[] body);
    }
}