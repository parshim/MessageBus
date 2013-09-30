using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBus : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPublisher CreatePublisher();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISubscriber CreateSubscriber();
    }
}
