using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class NullErrorSubscriber : IErrorSubscriber
    {
        public void UnhandledException(Exception exception)
        {
            
        }

        public void MessageDeserializeException(RawBusMessage busMessage, Exception exception)
        {  
        }

        public void MessageDispatchException(RawBusMessage busMessage, Exception exception)
        {
        }

        public void MessageFilteredOut(RawBusMessage busMessage)
        {
        }

        public void UnregisteredMessageArrived(RawBusMessage busMessage)
        {
        }
    }
}