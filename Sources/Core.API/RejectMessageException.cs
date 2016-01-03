using System;

namespace MessageBus.Core.API
{
    public class RejectMessageException : Exception
    {
        public object ReplyData { get; set; }
    }
}
