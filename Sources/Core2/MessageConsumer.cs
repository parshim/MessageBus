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
        private readonly bool _neverReply;
        private readonly string _replyExchange;

        private readonly TaskFactory _taskFactory;

        private readonly IMessageHelper _messageHelper;
        private readonly ISendHelper _sendHelper;
        private readonly Dictionary<string, ISerializer> _serializers;
        private readonly IErrorSubscriber _errorSubscriber;

        public MessageConsumer(string busId, IModel model, IMessageHelper messageHelper, ISendHelper sendHelper, Dictionary<string, ISerializer> serializers, IErrorSubscriber errorSubscriber, TaskScheduler scheduler, bool receiveSelfPublish, bool neverReply, string replyExchange)
            : base(model)
        {
            _busId = busId;
            _messageHelper = messageHelper;
            _serializers = serializers;
            _errorSubscriber = errorSubscriber;
            _receiveSelfPublish = receiveSelfPublish;
            _neverReply = neverReply;
            _sendHelper = sendHelper;
            _replyExchange = replyExchange;

            _taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskContinuationOptions.None, scheduler);
        }
        
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            Func<Task<bool>> func = () => ConsumeMessage(redelivered, deliveryTag, properties, body);

            _taskFactory.StartNew(func).Unwrap();
        }

        protected virtual async Task<bool> ConsumeMessage(bool redelivered, ulong deliveryTag, IBasicProperties properties, byte[] body)
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

                return false;
            }

            object data;

            if (!_serializers.ContainsKey(properties.ContentType))
            {
                RawBusMessage rawBusMessage = _messageHelper.ConstructMessage(dataContractKey, properties, (object)body);

                _errorSubscriber.MessageDeserializeException(rawBusMessage, new Exception("Unsupported content type"));

                return false;
            }

            ISerializer serializer;
            
            try
            {
                serializer = _serializers[properties.ContentType];
                
                data = serializer.Deserialize(dataContractKey, subscription.DataType, body);
            }
            catch (Exception ex)
            {
                RawBusMessage rawBusMessage = _messageHelper.ConstructMessage(dataContractKey, properties, (object)body);

                _errorSubscriber.MessageDeserializeException(rawBusMessage, ex);

                return false;
            }
            
            RawBusMessage message = _messageHelper.ConstructMessage(dataContractKey, properties, data);

            if (!_receiveSelfPublish && _busId.Equals(message.BusId))
            {
                _errorSubscriber.MessageFilteredOut(message);

                return false;
            }

            RawBusMessage reply;

            try
            {
                reply = await HandleMessage(subscription.Handler, message, redelivered, deliveryTag);
            }
            catch (RejectMessageException)
            {
                reply = new RawBusMessage();
                
                reply.Headers.Add(new RejectedHeader());
            }
            catch (Exception ex)
            {
                _errorSubscriber.MessageDispatchException(message, ex);

                reply = new RawBusMessage();

                reply.Headers.Add(new ExceptionHeader
                {
                    Message = ex.Message
                });
            }

            if (!_neverReply && properties.IsReplyToPresent())
            {
                _sendHelper.Send(new SendParams
                {
                    BusId = _busId,
                    BusMessage = reply,
                    Model = Model,
                    CorrelationId = properties.IsCorrelationIdPresent() ? properties.CorrelationId : "",
                    Exchange = _replyExchange,
                    RoutingKey = properties.ReplyTo,
                    Serializer = serializer,
                    MandatoryDelivery = false,
                    PersistentDelivery = false
                });
            }

            return true;
        }
        
        protected virtual Task<RawBusMessage> HandleMessage(ICallHandler handler, RawBusMessage message, bool redelivered, ulong deliveryTag)
        {
            return handler.Dispatch(message);
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