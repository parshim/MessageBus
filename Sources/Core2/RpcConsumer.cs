using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class CallbackInfo
    {
        private readonly Action<RawBusMessage, Exception> _callback;
        private readonly Type _replyType;

        public CallbackInfo(Action<RawBusMessage, Exception> callback, Type replyType)
        {
            _callback = callback;
            _replyType = replyType;
        }

        public Action<RawBusMessage, RpcCallException> Callback
        {
            get { return _callback; }
        }

        public Type ReplyType
        {
            get { return _replyType; }
        }
    }

    public class RpcConsumer : DefaultBasicConsumer, IRpcConsumer
    {
        private readonly ConcurrentDictionary<string, CallbackInfo> _callbacksDictionary = new ConcurrentDictionary<string, CallbackInfo>();

        private readonly IMessageHelper _messageHelper;
        private readonly Dictionary<string, ISerializer> _serializers;

        public RpcConsumer(IModel model, IMessageHelper messageHelper, Dictionary<string, ISerializer> serializers)
            : base(model)
        {
            _serializers = serializers;
            _messageHelper = messageHelper;
        }

        public bool RegisterCallback(string correlationId, Type replyType, Action<RawBusMessage, Exception> callback)
        {
            return _callbacksDictionary.TryAdd(correlationId, new CallbackInfo(callback, replyType));
        }

        public void HandleBasicReturn(string correlationId, int replyCode, string replyText)
        {
            CallbackInfo info;

            if (_callbacksDictionary.TryRemove(correlationId, out info))
            {
                info.Callback(null, new RpcCallException(RpcFailureReason.NotRouted, string.Format("Message not routed. Error code: {0}, reply: {1}", replyCode, replyText)));
            }
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            if (!properties.IsCorrelationIdPresent())
            {
                return;
            }

            CallbackInfo info;

            if (_callbacksDictionary.TryRemove(properties.CorrelationId, out info))
            {
                var rejectHeader =
                    properties.Headers.Where(pair => pair.Key == RejectedHeader.WellknownName)
                        .Select(pair => pair.Value)
                        .FirstOrDefault();

                if (rejectHeader != null)
                {
                    info.Callback(null, new RpcCallException(RpcFailureReason.Reject));

                    return;
                }

                var exceptionHeader =
                    properties.Headers.Where(pair => pair.Key == ExceptionHeader.WellknownName)
                        .Select(pair => pair.Value)
                        .FirstOrDefault();

                if (exceptionHeader != null)
                {
                    info.Callback(null, new RpcCallException(RpcFailureReason.HandlerError, exceptionHeader.ToString()));

                    return;
                }

                DataContractKey dataContractKey;
                object data;

                if (body.Length == 0 || info.ReplyType == null)
                {
                    // Void reply or sender not interested in reply data, but only interested to be notified that work is done

                    dataContractKey = DataContractKey.Void;
                    data = null;
                }
                else
                {
                    dataContractKey = properties.GetDataContractKey();

                    if (!_serializers.ContainsKey(properties.ContentType))
                    {
                        info.Callback(null,
                            new RpcCallException(RpcFailureReason.SerializationError,
                                string.Format("Unsupported content type {0}", properties.ContentType)));

                        return;
                    }

                    try
                    {
                        ISerializer serializer = _serializers[properties.ContentType];

                        data = serializer.Deserialize(dataContractKey, info.ReplyType, body);
                    }
                    catch (Exception ex)
                    {
                        info.Callback(null, new RpcCallException(RpcFailureReason.SerializationError, ex));

                        return;
                    }
                }

                RawBusMessage message = _messageHelper.ConstructMessage(dataContractKey, properties, data);

                info.Callback(message, null);
            }
        }

        public override void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
            _callbacksDictionary.Clear();
        }
    }
}