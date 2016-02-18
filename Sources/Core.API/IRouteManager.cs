using System;
using System.Collections.Generic;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Route manager provides an option to configure message routing
    /// </summary>
    public interface IRouteManager : IDisposable
    {
        /// <summary>
        /// Creates queue with suplied settings
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="autoDelete"></param>
        /// <param name="settings"></param>
        /// <returns>Name of the created queue</returns>
        string CreateQueue(string queueName, bool durable, bool autoDelete, CreateQueueSettings settings);
        
        /// <summary>
        /// Bind queue to receive specified message type from exchanged configured on bus
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        /// <param name="hierarchy"></param>
        /// <param name="filter"></param>
        void QueueBindMessage<T>(string queueName, string routingKey = "", bool hierarchy = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Bind queue to receive specified message type from provided exchanged
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="hierarchy"></param>
        /// <param name="filter"></param>
        void QueueBindMessage<T>(string queueName, string exchange, string routingKey = "", bool hierarchy = false, IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Bind queue to receive specified message type from provided exchanged
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="filter"></param>
        void QueueBindMessage(string queueName, string exchange, string routingKey = "", IEnumerable<BusHeader> filter = null);

        /// <summary>
        /// Deletes queue. All messages in deleted queue are lost.
        /// </summary>
        /// <param name="queueName"></param>
        void DeleteQueue(string queueName);
    }
}