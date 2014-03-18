using System.Collections.Generic;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Receive messages from the bus on demand, without handling message pump
    /// </summary>
    public interface IReceiver : ISubscription
    {
        /// <summary>
        /// Subscribe for message type.
        /// </summary>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <param name="hierarchy">Look for derived types and automaticaly register them for the same callback.</param>
        /// <param name="receiveSelfPublish">If true, messages of this type published whithin bus instance will be received and processed by subscriber. Otherwise ignored.</param>
        /// <param name="filter">Subscribe to message which sent only with specified headers.</param>
        /// <returns>True if sucessfuly subscribed, otherwise false.</returns>
        /// <exception cref="SubscribtionClosedException"></exception>
        bool Subscribe<TData>(bool hierarchy = false, bool receiveSelfPublish = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Receives next message in queue
        /// </summary>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <returns>Null if there is no messages queued</returns>
        TData Receive<TData>();

        /// <summary>
        /// Receives next message in queue
        /// </summary>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <returns>Null if there is no messages queued</returns>
        BusMessage<TData> ReceiveBusMessage<TData>();
    }
}