using System;
using System.Text;

using RabbitMQ.Client.Framing.v0_9;
using CommonFraming = RabbitMQ.Client.Framing.v0_9;

namespace MessageBus.Binding.RabbitMQ
{
    /// <summary>
    /// Properties of the current RabbitMQ Service Model Version
    /// </summary>
    public static class CurrentVersion
    {
        internal const String Scheme = "amqp";
        
        internal static Encoding DefaultEncoding { get { return Encoding.UTF8; } }

        internal static class StatusCodes
        {
            public const int Ok = Constants.ReplySuccess;
        }
    }
}
