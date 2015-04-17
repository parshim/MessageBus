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
        private readonly SubscriberConfigurator _configurator;

        private string _consumerTag;
        
        protected readonly ISubscriptionHelper _helper;

        public SubscriberBase(IModel model, string queue, IMessageConsumer consumer, SubscriberConfigurator configurator)
        {
            _model = model;

            _queue = queue;

            _consumer = consumer;
            _configurator = configurator;

            _helper = new SubscriptionHelper((type, filterInfo, handler) =>
            {
                if (_consumer.Register(type, filterInfo, handler))
                {
                    _model.QueueBind(_queue, configurator.Exchange, configurator.RoutingKey, filterInfo);

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
            _consumerTag = _model.BasicConsume(_queue, !_configurator.TransactionalDelivery, "", !_configurator.ReceiveSelfPublish, false, new Dictionary<string, object>(), _consumer);
        }

        public void Close()
        {
            _model.BasicCancel(_consumerTag);
        }
    }
}