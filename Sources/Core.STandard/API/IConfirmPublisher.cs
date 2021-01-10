using System;

namespace MessageBus.Core.API
{
    public interface IConfirmPublisher : IPublisher
    {
        /// <summary>
        /// Waits for confirms of all messages published till method call
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        bool WaitForConfirms(TimeSpan timeOut);
    }
}