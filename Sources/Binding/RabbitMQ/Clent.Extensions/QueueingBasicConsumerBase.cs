using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    public abstract class QueueingBasicConsumerBase : DefaultBasicConsumer, IMessageReceiver
    {
        protected SharedQueue<BasicDeliverEventArgs> _queue;

        ///<summary>Creates a fresh QueueingBasicConsumer,
        ///initializing the Model property to null and the Queue
        ///property to a fresh SharedQueue.</summary>
        protected QueueingBasicConsumerBase() : this(null) { }

        ///<summary>Creates a fresh QueueingBasicConsumer, with Model
        ///set to the argument, and Queue set to a fresh
        ///SharedQueue.</summary>
        protected QueueingBasicConsumerBase(IModel model) : this(model, new SharedQueue<BasicDeliverEventArgs>()) { }

        ///<summary>Creates a fresh QueueingBasicConsumer,
        ///initializing the Model and Queue properties to the given
        ///values.</summary>
        protected QueueingBasicConsumerBase(IModel model, SharedQueue<BasicDeliverEventArgs> queue)
            : base(model)
        {
            _queue = queue;
        }

        ///<summary>Retrieves the SharedQueue that messages arrive on.</summary>
        public SharedQueue<BasicDeliverEventArgs> Queue
        {
            get { return _queue; }
        }

        ///<summary>Overrides DefaultBasicConsumer's OnCancel
        ///implementation, extending it to call the Close() method of
        ///the SharedQueue.</summary>
        public override void OnCancel()
        {
            _queue.Close();
            base.OnCancel();
        }

        ///<summary>Overrides DefaultBasicConsumer's
        ///HandleBasicDeliver implementation, building a
        ///BasicDeliverEventArgs instance and placing it in the
        ///Queue.</summary>
        public override void HandleBasicDeliver(string consumerTag,
                                                ulong deliveryTag,
                                                bool redelivered,
                                                string exchange,
                                                string routingKey,
                                                IBasicProperties properties,
                                                byte[] body)
        {
            BasicDeliverEventArgs e = new BasicDeliverEventArgs
                {
                    ConsumerTag = consumerTag,
                    DeliveryTag = deliveryTag,
                    Redelivered = redelivered,
                    Exchange = exchange,
                    RoutingKey = routingKey,
                    BasicProperties = properties,
                    Body = body
                };

            _queue.Enqueue(e);
        }

        public BasicGetResult Receive(TimeSpan timeout)
        {
            BasicDeliverEventArgs message;

            bool dequeue = Dequeue(timeout, out message);

            if (!dequeue) return null;

            return new BasicGetResult(message.DeliveryTag, message.Redelivered, message.Exchange, message.RoutingKey, 0, message.BasicProperties, message.Body);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return _queue.WaitForMessage(timeout);
        }

        public bool Dequeue(TimeSpan timeout, out BasicDeliverEventArgs message)
        {
            return _queue.Dequeue(timeout, out message);
        }

        public BasicDeliverEventArgs DequeueNoWait()
        {
            return _queue.DequeueNoWait(null);
        }

        public BasicDeliverEventArgs Dequeue()
        {
            return _queue.Dequeue();
        }

        public abstract void DropMessage(ulong deliveryTag);

        public abstract void AcceptMessage(ulong deliveryTag);
    }
}