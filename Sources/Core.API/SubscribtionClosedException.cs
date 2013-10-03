using System;

namespace MessageBus.Core.API
{
    public class SubscribtionClosedException : Exception
    {
        public SubscribtionClosedException()
            : base("Unable to subscribe to message types after subscriber start to process messages")
        {
        }
    }
}