using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Logger provider for subsriber exceptional cases
    /// </summary>
    public interface IErrorSubscriber
    {
        /// <summary>
        /// General unhendled exception
        /// </summary>
        /// <param name="exception"></param>
        void UnhandledException(Exception exception);

        /// <summary>
        /// Message serialization failed
        /// </summary>
        /// <param name="busMessage"></param>
        /// <param name="exception"></param>
        void MessageDeserializeException(RawBusMessage busMessage, Exception exception);

        /// <summary>
        /// Message handler throws exception
        /// </summary>
        /// <param name="busMessage"></param>
        /// <param name="exception"></param>
        void MessageDispatchException(RawBusMessage busMessage, Exception exception);

        /// <summary>
        /// Message filtered
        /// </summary>
        /// <param name="busMessage"></param>
        void MessageFilteredOut(RawBusMessage busMessage);

        /// <summary>
        /// Unregistered message arrived
        /// </summary>
        /// <param name="busMessage"></param>
        void UnregisteredMessageArrived(RawBusMessage busMessage);
    }
}