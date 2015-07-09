using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{

    public interface IRpcConsumer : IBasicConsumer
    {
        bool RegisterCallback(string correlationId, Type replyType, Action<RawBusMessage, Exception> callback);
        void HandleBasicReturn(string correlationId, int replyCode, string replyText);
    }
}