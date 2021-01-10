using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// RPC publisher is capable of sending messages and wait for reply message on same channel without creating or using any queue for reply message.
    /// </summary>
    public interface IRpcPublisher : IPublisher
    {
        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="data"></param>
        /// <param name="timeOut">Specify for how long wait for reply message. If message not received within specified time timeout exception will be thrown.</param>
        /// <param name="persistant"></param>
        /// <returns>Reply data</returns>
        TReplyData Send<TData, TReplyData>(TData data, TimeSpan timeOut, bool persistant = false);

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        /// <param name="timeOut">Specify for how long wait for reply message. If message not received within specified time timeout exception will be thrown.</param>
        /// <param name="persistant"></param>
        /// <remarks>Message returned without any content, or if content exists it is not deserialize</remarks>
        void Send<TData>(TData data, TimeSpan timeOut, bool persistant = false);

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="message"></param>
        /// <param name="timeOut">Specify for how long wait for reply message. If message not received within specified time timeout exception will be thrown.</param>
        /// <param name="persistant"></param>
        /// <returns>Reply message</returns>
        BusMessage<TReplyData> Send<TData, TReplyData>(BusMessage<TData> message, TimeSpan timeOut, bool persistant = false);

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="message"></param>
        /// <param name="timeOut">Specify for how long wait for reply message. If message not received within specified time timeout exception will be thrown.</param>
        /// <param name="persistant"></param>
        /// <remarks>Message returned without any content, or if content exists it is not deserialize</remarks>
        void Send<TData>(BusMessage<TData> message, TimeSpan timeOut, bool persistant = false);

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="message">Outbound message</param>
        /// <param name="timeOut">Specify for how long wait for reply message. If message not received within specified time action will not be executed.</param>
        /// <param name="onReply">Action to perform whenever reply is being received</param>
        /// <param name="persistant"></param>
        /// <returns>Reply message</returns>
        void Send<TData, TReplyData>(BusMessage<TData> message, TimeSpan timeOut, Action<BusMessage<TReplyData>> onReply, bool persistant = false);

        /// <summary>
        /// Publish data message and waits for response data message on same channel
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TReplyData"></typeparam>
        /// <param name="data">Outbound message</param>
        /// <param name="timeOut">Specify for how long wait for reply message. If message not received within specified time action will not be executed.</param>
        /// <param name="onReply">Action to perform whenever reply is being received</param>
        /// <param name="persistant"></param>
        /// <returns>Reply message</returns>
        void Send<TData, TReplyData>(TData data, TimeSpan timeOut, Action<TReplyData> onReply, bool persistant = false);
    }
}