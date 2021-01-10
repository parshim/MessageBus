namespace MessageBus.Core.API
{
    /// <summary>
    /// Trace all incoming and outgoing messages
    /// </summary>
    public interface ITrace
    {
        /// <summary>
        /// Message arrived
        /// </summary>
        /// <param name="busId"></param>
        /// <param name="busMessage"></param>
        /// <param name="consumerTag"></param>
        void MessageArrived(string busId, RawBusMessage busMessage, string consumerTag);

        /// <summary>
        /// Message sent
        /// </summary>
        /// <param name="busId"></param>
        /// <param name="busMessage"></param>
        void MessageSent(string busId, RawBusMessage busMessage);
    }
}