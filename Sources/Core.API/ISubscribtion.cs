using System;

namespace MessageBus.Core.API
{
    public interface ISubscribtion : IDisposable
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