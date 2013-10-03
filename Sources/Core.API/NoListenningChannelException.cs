using System;

namespace MessageBus.Core.API
{
    public class NoListenningChannelException : Exception
    {
        public NoListenningChannelException() : base("Unable to open subscriber cause any input connection was accepted")
        {
        }
    }
}
