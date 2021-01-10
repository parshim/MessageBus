using System;

namespace MessageBus.Core.API
{
    public class NoIncomingConnectionAcceptedException : Exception
    {
        public NoIncomingConnectionAcceptedException() : base("Unable to open subscriber cause any input connection was accepted")
        {
        }
    }
}
