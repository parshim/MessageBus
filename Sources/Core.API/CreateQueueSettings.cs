using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Settings for requested create queue operation
    /// </summary>
    public class CreateQueueSettings
    {
        public static CreateQueueSettings Default = new CreateQueueSettings
        {
            TTL = TimeSpan.Zero,
            AutoExpire = TimeSpan.Zero,
            DeadLetterExchange = "",
            DeadLetterRoutingKey = "",
            MaxLength = 0,
            MaxPriority = 0,
            MaxSizeBytes = 0
        };

        /// <summary>
        /// How long a message published to a queue can live before it is discarded
        /// </summary>
        public TimeSpan TTL { get; set; }

        /// <summary>
        /// How long a queue can be unused for before it is automatically deleted
        /// </summary>
        public TimeSpan AutoExpire { get; set; }

        /// <summary>
        /// How many (ready) messages a queue can contain before it starts to drop them from its head
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Total body size for ready messages a queue can contain before it starts to drop them from its head.
        /// </summary>
        public int MaxSizeBytes { get; set; }

        /// <summary>
        /// Maximum number of priority levels for the queue to support; if not set, the queue will not support message priorities
        /// </summary>
        public int MaxPriority { get; set; }

        /// <summary>
        /// Optional name of an exchange to which messages will be republished if they are rejected or expire
        /// </summary>
        public string DeadLetterExchange { get; set; }

        /// <summary>
        /// Optional replacement routing key to use when a message is dead-lettered. If this is not set, the message's original routing key will be used.
        /// </summary>
        public string DeadLetterRoutingKey { get; set; }
    }
}