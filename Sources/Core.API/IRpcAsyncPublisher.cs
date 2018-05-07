using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Core.API
{
    /// <summary>
    /// RPC publisher is capable of sending messages and wait for reply message on same channel without creating or using any queue for reply message.
    /// </summary>
    public interface IRpcAsyncPublisher : IPublisher
    {
        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="data"></param>
        /// <param name="cancellationToken">Cancallation token that will be used to cancel asyncronous task before it is being completed.</param>
        /// <param name="persistant"></param>
        /// <returns>Reply data</returns>
        Task<TReplyData> Send<TData, TReplyData>(TData data, bool persistant = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        /// <param name="cancellationToken">Cancallation token that will be used to cancel asyncronous task before it is being completed.</param>
        /// <param name="persistant"></param>
        /// <remarks>Message returned without any content, or if content exists it is not deserialize</remarks>
        Task Send<TData>(TData data, bool persistant = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken">Cancallation token that will be used to cancel asyncronous task before it is being completed.</param>
        /// <param name="persistant"></param>
        /// <returns>Reply message</returns>
        Task<BusMessage<TReplyData>> Send<TData, TReplyData>(BusMessage<TData> message, bool persistant = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken">Cancallation token that will be used to cancel asyncronous task before it is being completed.</param>
        /// <param name="persistant"></param>
        /// <remarks>Message returned without any content, or if content exists it is not deserialize</remarks>
        Task Send<TData>(BusMessage<TData> message, bool persistant = false, CancellationToken cancellationToken = default(CancellationToken));
    }
}