using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// RPC publisher is capable of sending messages and wait for reply message on same channel without creating or using any queue for reply message.
    /// </summary>
    public interface IRpcPublisher : IDisposable
    {
        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="data"></param>
        /// <param name="timeOut">Specify for how long wait for reply message</param>
        /// <param name="reply">Reply data</param>
        /// <returns>True when reply were received</returns>
        bool Send<TData, TReplyData>(TData data, TimeSpan timeOut, out TReplyData reply);

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="message"></param>
        /// <param name="timeOut">Specify for how long wait for reply message</param>
        /// <param name="reply">Reply message</param>
        /// <returns>True when reply were received</returns>
        bool Send<TData, TReplyData>(BusMessage<TData> message, TimeSpan timeOut, out BusMessage<TReplyData> reply);
    }
}