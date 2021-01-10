using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class NullExceptionFilter : IExceptionFilter
    {
        public bool Filter(Exception exception, RawBusMessage message, bool redelivered, ulong deliveryTag)
        {
            return true;
        }
    }
}