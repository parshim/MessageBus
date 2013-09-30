using System;
using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class RabbitMQBus : Bus
    {
        public RabbitMQBus()
            : this(Guid.NewGuid().ToString())
        {
        }
        
        public RabbitMQBus(string busId)
            : this(busId, "localhost", "amq.fanout", false, new NullErrorSubscriber())
        {
        }

        public RabbitMQBus(string busId, IErrorSubscriber errorSubscriber)
            : this(busId, "localhost", "amq.fanout", false, errorSubscriber)
        {
        }

        public RabbitMQBus(string busId, string host, string exchange, bool exactlyOnce, IErrorSubscriber errorSubscriber)
            : base(new RabbitMQBinding
                {
                    ApplicationId = busId,
                    IgnoreSelfPublished = false,
                    AutoBindExchange = exchange,
                    OneWayOnly = true,
                    ExactlyOnce = exactlyOnce,
                    PersistentDelivery = false
                }, new Uri(string.Format("amqp://{0}/{1}", host, exchange)), new Uri(string.Format("amqp://{0}/", host)), busId, errorSubscriber)
        {
        }
    }
}