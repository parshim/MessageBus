using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Defines which exception should reject messages and which should requeue
    /// </summary>
    public interface IExceptionFilter
    {
        bool Filter(Exception exception, RawBusMessage message, bool redelivered, ulong deliveryTag);
    }
}