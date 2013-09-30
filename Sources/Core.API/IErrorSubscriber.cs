using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// 
    /// </summary>
    public interface IErrorSubscriber
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="busMessage"></param>
        /// <param name="exception"></param>
        void MessageDeserializeException(RawBusMessage busMessage, Exception exception);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="busMessage"></param>
        /// <param name="exception"></param>
        void MessageDispatchException(RawBusMessage busMessage, Exception exception);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="busMessage"></param>
        void MessageFilteredOut(RawBusMessage busMessage);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="busMessage"></param>
        void UnregisteredMessageArrived(RawBusMessage busMessage);
    }
}