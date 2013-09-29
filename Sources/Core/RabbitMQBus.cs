using System;
using MessageBus.Binding.RabbitMQ;

namespace MessageBus.Core
{
    public class RabbitMQBus : Bus
    {
        public RabbitMQBus()
            : this("localhost", "amq.fanout", false)
        {
        }

        public RabbitMQBus(string host, string exchange, bool exactlyOnce)
            : base(new RabbitMQBinding
                {
                    ApplicationId = Guid.NewGuid().ToString(),
                    IgnoreSelfPublished = true,
                    AutoBindExchange = exchange,
                    OneWayOnly = true,
                    ExactlyOnce = exactlyOnce,
                    PersistentDelivery = false
                }, new Uri(string.Format("amqp://{0}/{1}", host, exchange)), new Uri(string.Format("amqp://{0}/", host)))
        {
        }
    }
}