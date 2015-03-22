using System.Collections.Generic;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class SubscriberBase : ISubscription
    {
        private readonly IModel _model;
        private readonly string _queue;
        private readonly IMessageConsumer _consumer;
        private readonly bool _receiveSelfPublish;

        private string _consumerTag;
        
        protected readonly ISubscriptionHelper _helper;

        public SubscriberBase(IModel model, string exchange, string queue, IMessageConsumer consumer, bool receiveSelfPublish)
        {
            _model = model;

            _queue = queue;

            _consumer = consumer;
            _receiveSelfPublish = receiveSelfPublish;

            _helper = new SubscriptionHelper((type, filterInfo, handler) =>
            {
                if (_consumer.Register(type, filterInfo, handler))
                {
                    _model.QueueBind(_queue, exchange, filterInfo);

                    return true;
                }

                return false;
            });
        }

        public void Dispose()
        {
            _model.Close();
        }

        public void Open()
        {
            _consumerTag = _model.BasicConsume(_queue, true, "", !_receiveSelfPublish, false, new Dictionary<string, object>(), _consumer);
        }

        public void Close()
        {
            _model.BasicCancel(_consumerTag);
        }
    }
}