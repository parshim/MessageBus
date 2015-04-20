using System;
using MessageBus.Core.API;
using Microsoft.AspNet.SignalR.Messaging;

namespace Core.SignalR
{
    public class ScaleoutMessageSerializer : ISerializer
    {
        public string ContentType { get { return "application/octet-stream"; } }

        public byte[] Serialize(RawBusMessage data)
        {
            return ((ScaleoutMessage)data.Data).ToBytes();
        }

        public object Deserialize(DataContractKey dataContractKey, Type dataType, byte[] body)
        {
            return ScaleoutMessage.FromBytes(body);
        }
    }
}