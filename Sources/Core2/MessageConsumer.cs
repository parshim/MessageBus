using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class MessageConsumer : DefaultBasicConsumer, IMessageConsumer
    {
        private readonly ConcurrentDictionary<Type, SubscriptionInfo> _subscriptions = new ConcurrentDictionary<Type, SubscriptionInfo>();

        private readonly string _busId;
        private readonly bool _receiveSelfPublish;

        private readonly TaskScheduler _scheduler;

        private readonly IMessageHelper _messageHelper;
        private readonly Dictionary<string, ISerializer> _serializers;
        private readonly IErrorSubscriber _errorSubscriber;
        
        public MessageConsumer(IModel model, string busId, IMessageHelper messageHelper, Dictionary<string, ISerializer> serializers, IErrorSubscriber errorSubscriber, TaskScheduler scheduler, bool receiveSelfPublish) : base(model)
        {
            _busId = busId;
            _messageHelper = messageHelper;
            _serializers = serializers;
            _errorSubscriber = errorSubscriber;
            _scheduler = scheduler;
            _receiveSelfPublish = receiveSelfPublish;
        }
        
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            Task.Factory.StartNew(() => ConsumeMessage(redelivered, deliveryTag, properties, body), CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }
        
        private void ConsumeMessage(bool redelivered, ulong deliveryTag, IBasicProperties properties, byte[] body)
        {
            DataContractKey dataContractKey = properties.GetDataContractKey();

            var subscription =
                _subscriptions.Where(p => p.Value.FilterInfo.ContractKey.Equals(dataContractKey)).Select(
                    pair =>
                        new
                        {
                            DataType = pair.Key,
                            pair.Value.Handler
                        }).FirstOrDefault();

            if (subscription == null)
            {
                 RawBusMessage rawBusMessage = _messageHelper.ConstructMessage(dataContractKey, properties, (object)body);

                _errorSubscriber.UnregisteredMessageArrived(rawBusMessage);

                return;
            }

            object data;

            if (!_serializers.ContainsKey(properties.ContentType))
            {
                RawBusMessage rawBusMessage = _messageHelper.ConstructMessage(dataContractKey, properties, (object)body);

                _errorSubscriber.MessageDeserializeException(rawBusMessage, new Exception("Unsupported content type"));

                return;
            }

            try
            {
                data = _serializers[properties.ContentType].Deserialize(dataContractKey, subscription.DataType, body);
            }
            catch (Exception ex)
            {
                RawBusMessage rawBusMessage = _messageHelper.ConstructMessage(dataContractKey, properties, (object)body);

                _errorSubscriber.MessageDeserializeException(rawBusMessage, ex);

                return;
            }
            
            RawBusMessage message = _messageHelper.ConstructMessage(dataContractKey, properties, data);

            if (!_receiveSelfPublish && _busId.Equals(message.BusId))
            {
                _errorSubscriber.MessageFilteredOut(message);

                return;
            }

            try
            {
                HandleMessage(subscription.Handler, message, redelivered, deliveryTag);
            }
            catch (Exception ex)
            {
                _errorSubscriber.MessageDispatchException(message, ex);
            }
        }

        protected virtual void HandleMessage(ICallHandler handler, RawBusMessage message, bool redelivered, ulong deliveryTag)
        {
            handler.Dispatch(message);
        }
        
        public bool Register(Type type, MessageFilterInfo filterInfo, ICallHandler handler)
        {
            return _subscriptions.TryAdd(type, new SubscriptionInfo
            {
                FilterInfo = filterInfo,
                Handler = handler
            });
        }
    }
}