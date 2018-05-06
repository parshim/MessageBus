using System;
using System.Collections.Generic;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class SubscriberBase : ISubscription
    {
        private readonly IModel _model;
        private readonly string _queue;
        private readonly IBasicConsumer _consumer;
        private readonly SubscriberConfigurator _configurator;

        private string _consumerTag;
        
        public SubscriberBase(IModel model, string queue, IBasicConsumer consumer, SubscriberConfigurator configurator)
        {
            _model = model;

            _queue = queue;

            _consumer = consumer;
            _configurator = configurator;
        }

        ~SubscriberBase()
        {
            Dispose(true);
        }

        private void Dispose(bool finializing)
        {
            if (finializing && _configurator.Blocked) return;

            _model.Abort();
        }

        public void Dispose()
        {
            Dispose(false);
            
            GC.SuppressFinalize(this);
        }

        public void Open()
        {
            if (_configurator.Prefetch > 0)
            {
                _model.BasicQos(0, _configurator.Prefetch, false);
            }

            _consumerTag = _model.BasicConsume(_queue, !_configurator.TransactionalDelivery, _configurator.ConsumerTag, !_configurator.ReceiveSelfPublish, false, new Dictionary<string, object>(), _consumer);
        }

        public void Close()
        {
            _model.BasicCancel(_consumerTag);
        }
    }
}