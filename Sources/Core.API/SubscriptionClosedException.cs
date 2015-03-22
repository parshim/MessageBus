using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// 
    /// </summary>
    public class SubscriptionClosedException : Exception
    {
        public SubscriptionClosedException()
            : base("Unable to subscribe to message types after subscriber start to process messages")
        {
        }
    }
}