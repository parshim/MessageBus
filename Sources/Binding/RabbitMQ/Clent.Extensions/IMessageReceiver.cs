using System;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    interface IMessageReceiver
    {
        /// <summary>
        /// Returns next message in queue 
        /// </summary>
        /// <param name="timeout">Maximum wait time</param>
        /// <returns></returns>
        BasicGetResult Receive(TimeSpan timeout);

        /// <summary>
        /// Waits till queue will receive any message. Blocks calling thread till queue have at least one message or till timeout.
        /// </summary>
        /// <param name="timeout">Maximum wait time</param>
        /// <returns>True if queue have at least one message at method call or till timeout any message enquired</returns>
        bool WaitForMessage(TimeSpan timeout);

        /// <summary>
        /// Ensure that message with suplied delivery tag will not be redelivered
        /// </summary>
        /// <param name="deliveryTag"></param>
        void DropMessage(ulong deliveryTag);

        /// <summary>
        /// Message is accepted for processing
        /// </summary>
        /// <param name="deliveryTag"></param>
        void AcceptMessage(ulong deliveryTag);
    }
}
