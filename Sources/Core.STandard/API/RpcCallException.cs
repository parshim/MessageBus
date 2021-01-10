using System;

namespace MessageBus.Core.API
{
    public enum RpcFailureReason
    {
        TimeOut,
        HandlerError,
        Reject,
        SerializationError,
        NotRouted
    }

    public class RpcCallException : Exception
    {
        public RpcCallException(RpcFailureReason reason)
        {
            Reason = reason;
        }
        
        public RpcCallException(RpcFailureReason reason, object replyData)
        {
            Reason = reason;
            ReplyData = replyData;
        }

        public RpcCallException(RpcFailureReason reason, string message) :base(message)
        {
            Reason = reason;
        }

        public RpcCallException(RpcFailureReason reason, Exception innerException):base("", innerException)
        {
            Reason = reason;
        }

        public RpcFailureReason Reason { get; private set; }

        public object ReplyData { get; private set; }
    }
}