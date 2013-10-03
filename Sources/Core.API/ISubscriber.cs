using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Provides a functionality to subscribe to specific message types and process them in dispatching order.
    /// </summary>
    public interface ISubscriber : IDisposable
    {
        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriver.</param>
        /// <param name="hierarchy">Look for derived types and automaticaly register them for the same callback.</param>
        /// <param name="receiveSelfPublish">If true, messages of this type published whithin bus instance will be received and processed by subscriber. Otherwise ignored.</param>
        /// <returns>True if sucessfuly subscribed, otherwise false.</returns>
        /// <exception cref="SubscribtionClosedException"></exception>
        bool Subscribe<TData>(Action<TData> callback, bool hierarchy = false, bool receiveSelfPublish = false);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriver.</param>
        /// <param name="hierarchy">Look for derived types and automaticaly register them for the same callback.</param>
        /// <param name="receiveSelfPublish">If true, messages of this type published whithin bus instance will be received and processed by subscriber. Otherwise ignored.</param>
        /// <returns>True if sucessfuly subscribed, otherwise false.</returns>
        /// <exception cref="SubscribtionClosedException"></exception>
        bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy = false, bool receiveSelfPublish = false);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <param name="dataType">Message data contract type.</param>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriver.</param>
        /// <param name="hierarchy">Look for derived types and automaticaly register them for the same callback.</param>
        /// <param name="receiveSelfPublish">If true, messages of this type published whithin bus instance will be received and processed by subscriber. Otherwise ignored.</param>
        /// <returns>True if sucessfuly subscribed, otherwise false.</returns>
        /// <exception cref="SubscribtionClosedException"></exception>
        bool Subscribe(Type dataType, Action<object> callback, bool hierarchy = false, bool receiveSelfPublish = false);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <param name="dataType">Message data contract type.</param>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriver.</param>
        /// <param name="hierarchy">Look for derived types and automaticaly register them for the same callback.</param>
        /// <param name="receiveSelfPublish">If true, messages of this type published whithin bus instance will be received and processed by subscriber. Otherwise ignored.</param>
        /// <returns>True if sucessfuly subscribed, otherwise false.</returns>
        /// <exception cref="SubscribtionClosedException"></exception>
        bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy = false, bool receiveSelfPublish = false);

        /// <summary>
        /// Start process subscribed message types. No more subscribtion possible after that point.
        /// </summary>
        void StartProcessMessages();
    }
}