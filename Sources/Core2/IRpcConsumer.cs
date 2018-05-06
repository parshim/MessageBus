using System;
using System.Threading;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{

    public interface IRpcConsumer : IBasicConsumer
    {
        WaitHandle RegisterCallback(string correlationId, Type replyType, Action<RawBusMessage, Exception> callback);

        void RemoveCallback(string correlationId);

        void HandleBasicReturn(string correlationId, int replyCode, string replyText);
    }
}