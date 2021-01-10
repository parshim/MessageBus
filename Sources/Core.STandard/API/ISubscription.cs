using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Subscription interface provides basic interface to control receive messages
    /// </summary>
    public interface ISubscription : IDisposable
    {
        /// <summary>
        /// Start process subscribed message types.
        /// </summary>
        void Open();

        /// <summary>
        /// Stop process messages
        /// </summary>
        void Close();
    }
}