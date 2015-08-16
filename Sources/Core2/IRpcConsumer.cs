using System;
using System.Threading;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{

    public interface IRpcConsumer : IBasicConsumer
    {
        WaitHandle RegisterCallback(string correlationId, Type replyType, TimeSpan timeOut, Action<RawBusMessage, Exception> callback);

        void HandleBasicReturn(string correlationId, int replyCode, string replyText);
    }
}