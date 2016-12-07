﻿using System;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RouteManager : IRouteManager
    {
        private readonly string _exchange;
        private readonly IModel _model;

        public RouteManager(IModel model, string exchange)
        {
            _model = model;
            _exchange = exchange;
        }

        public void Dispose()
        {
            _model.Dispose();
        }

        public string CreateQueue(string name, bool durable, bool autoDelete, CreateQueueSettings settings)
        {
            var arguments = new Dictionary<string, object>();

            if (settings.TTL != TimeSpan.Zero)
            {
                arguments.Add("x-message-ttl", (int)settings.TTL.TotalMilliseconds);
            }

            if (settings.AutoExpire != TimeSpan.Zero)
            {
                arguments.Add("x-expires", (int)settings.AutoExpire.TotalMilliseconds);
            }

            if (settings.MaxLength > 0)
            {
                arguments.Add("x-max-length", settings.MaxLength);
            }

            if (settings.MaxSizeBytes > 0)
            {
                arguments.Add("x-max-length-bytes", settings.MaxSizeBytes);
            }

            if (settings.MaxPriority > 0)
            {
                arguments.Add("x-max-priority", settings.MaxPriority);
            }

            if (!string.IsNullOrEmpty(settings.DeadLetterExchange))
            {
                arguments.Add("x-dead-letter-exchange", settings.DeadLetterExchange);
            }

            if (!string.IsNullOrEmpty(settings.DeadLetterRoutingKey))
            {
                arguments.Add("x-dead-letter-routing-key", settings.DeadLetterRoutingKey);
            }

            QueueDeclareOk declareOk = _model.QueueDeclare(name, durable, false, autoDelete, arguments);

            return declareOk.QueueName;
        }

        public void QueueBindMessage<T>(string queueName, string routingKey, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            QueueBindMessage<T>(queueName, _exchange, routingKey, hierarchy, filter);
        }

        public void QueueBindMessage<T>(string queueName, string exchange, string routingKey = "", bool hierarchy = false, IEnumerable<BusHeader> filter = null)
        {
            var helper = new SubscriptionHelper((type, filterInfo, handler) =>
            {
                _model.QueueBind(queueName, exchange, routingKey, filterInfo);

                return true;
            });

            helper.Subscribe(typeof(T), new NullCallHandler(), hierarchy, filter);
        }

        public void QueueBindMessage(string queueName, string exchange, string routingKey = "", IEnumerable<BusHeader> filter = null)
        {
            _model.QueueBind(queueName, exchange, routingKey, filter ?? Enumerable.Empty<BusHeader>());
        }

        public void DeleteQueue(string name)
        {
            _model.QueueDelete(name);
        }
    }
}