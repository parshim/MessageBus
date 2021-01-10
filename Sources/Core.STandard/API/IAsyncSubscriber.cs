using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Provides a functionality to subscribe to specific message types and process them in dispatching order.
    /// </summary>
    public interface IAsyncSubscriber : ISubscription
    {
        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriber.</param>
        /// <param name="hierarchy">Look for derived types and automatically register them for the same callback.</param>
        /// <param name="filter">Subscribe to message which sent only with specified headers.</param>
        /// <returns>True if successfully subscribed, otherwise false.</returns>
        /// <exception cref="SubscriptionClosedException"></exception>
        bool Subscribe<TData>(Func<TData, Task> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriber.</param>
        /// <param name="hierarchy">Look for derived types and automatically register them for the same callback.</param>
        /// <param name="filter">Subscribe to message which sent only with specified headers.</param>
        /// <returns>True if successfully subscribed, otherwise false.</returns>
        /// <exception cref="SubscriptionClosedException"></exception>
        bool Subscribe<TData>(Func<BusMessage<TData>, Task> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <typeparam name="TReplyData">Reply message data contract type.</typeparam>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriber.</param>
        /// <param name="hierarchy">Look for derived types and automatically register them for the same callback.</param>
        /// <param name="filter">Subscribe to message which sent only with specified headers.</param>
        /// <returns>True if successfully subscribed, otherwise false.</returns>
        /// <exception cref="SubscriptionClosedException"></exception>
        bool Subscribe<TData, TReplyData>(Func<TData, Task<TReplyData>> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <typeparam name="TData">Message data contract type.</typeparam>
        /// <typeparam name="TReplyData">Reply message data contract type.</typeparam>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriber.</param>
        /// <param name="hierarchy">Look for derived types and automatically register them for the same callback.</param>
        /// <param name="filter">Subscribe to message which sent only with specified headers.</param>
        /// <returns>True if successfully subscribed, otherwise false.</returns>
        /// <exception cref="SubscriptionClosedException"></exception>
        bool Subscribe<TData, TReplyData>(Func<BusMessage<TData>, Task<BusMessage<TReplyData>>> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <param name="dataType">Message data contract type.</param>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriber.</param>
        /// <param name="hierarchy">Look for derived types and automatically register them for the same callback.</param>
        /// <param name="filter">Subscribe to message which sent only with specified headers.</param>
        /// <returns>True if successfully subscribed, otherwise false.</returns>
        /// <exception cref="SubscriptionClosedException"></exception>
        bool Subscribe(Type dataType, Func<object, Task> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Subscribe for message type. Specified callback will be called every time message with provided type will received. 
        /// </summary>
        /// <remarks>All messages are processed in the same message pump thread to preserve message order.</remarks>
        /// <param name="dataType">Message data contract type.</param>
        /// <param name="callback">Callback to call the message of specified type has been received by subscriber.</param>
        /// <param name="hierarchy">Look for derived types and automatically register them for the same callback.</param>
        /// <param name="filter">Subscribe to message which sent only with specified headers.</param>
        /// <returns>True if successfully subscribed, otherwise false.</returns>
        /// <exception cref="SubscriptionClosedException"></exception>
        bool Subscribe(Type dataType, Func<RawBusMessage, Task> callback, bool hierarchy = false, IEnumerable<BusHeader> filter = null);
    }
}