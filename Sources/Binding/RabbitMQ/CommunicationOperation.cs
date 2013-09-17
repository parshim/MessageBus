using System;
using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
    internal delegate void CommunicationOperation(TimeSpan timeout);
    internal delegate TResult CommunicationOperation<out TResult>(TimeSpan timeout);
    internal delegate TResult CommunicationOperation<out TResult, TArg>(TimeSpan timeout, out TArg arg0);
    internal delegate void SendOperation(Message message, TimeSpan timeout);
}