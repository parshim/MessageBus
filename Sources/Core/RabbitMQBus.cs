using System;
using MessageBus.Binding.RabbitMQ;

namespace MessageBus.Core
{
    public class RabbitMQBus : Bus
    {
        public RabbitMQBus()
            : this("amq.fanout", false)
        {
        }

        public RabbitMQBus(string exchange, bool exactlyOnce)
            : base(new RabbitMQBinding
                {
                    ApplicationId = Guid.NewGuid().ToString(),
                    IgnoreSelfPublished = true,
                    AutoBindExchange = exchange,
                    OneWayOnly = true,
                    ExactlyOnce = exactlyOnce,
                    PersistentDelivery = false
                })
        {
        }
    }
}