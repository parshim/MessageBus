using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class CallbackInfo
    {
        private readonly Action<RawBusMessage, Exception> _callback;

        private readonly Type _replyType;
        private readonly ManualResetEvent _ev;

        private RawBusMessage _message;
        private Exception _exception;

        public CallbackInfo(Action<RawBusMessage, Exception> callback, Type replyType)
        {
            _callback = callback;
            _replyType = replyType;
        }

        public CallbackInfo(Action<RawBusMessage, Exception> callback, Type replyType, ManualResetEvent ev)
        {
            _callback = callback;
            _replyType = replyType;
            _ev = ev;
        }

        public void SetResponse(RawBusMessage message, Exception exception)
        {
            _message = message;
            _exception = exception;

            _callback(_message, _exception);

            _ev.Set();
        }

        public Type ReplyType
        {
            get { return _replyType; }
        }

        public WaitHandle WaitHandle
        {
            get { return _ev; }
        }
    }

    public class RpcConsumer : DefaultBasicConsumer, IRpcConsumer
    {
        private readonly ConcurrentDictionary<string, CallbackInfo> _callbacksDictionary = new ConcurrentDictionary<string, CallbackInfo>();

        private readonly IMessageHelper _messageHelper;
        private readonly Dictionary<string, ISerializer> _serializers;
        private readonly ITrace _trace;
        private readonly string _busId;

        public RpcConsumer(string busId, IModel model, IMessageHelper messageHelper, Dictionary<string, ISerializer> serializers, ITrace trace)
            : base(model)
        {
            _busId = busId;
            _serializers = serializers;
            _trace = trace;
            _messageHelper = messageHelper;
        }

        public WaitHandle RegisterCallback(string correlationId, Type replyType, Action<RawBusMessage, Exception> callback)
        {
            CallbackInfo callbackInfo = _callbacksDictionary.GetOrAdd(correlationId, id => CreateCallback(replyType, callback));

            return callbackInfo.WaitHandle;
        }

        public void RemoveCallback(string correlationId)
        {
            CallbackInfo val;
            _callbacksDictionary.TryRemove(correlationId, out val);
        }

        private CallbackInfo CreateCallback(Type replyType, Action<RawBusMessage, Exception> callback)
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            
            return new CallbackInfo(callback, replyType, ev);
        }
        
        public void HandleBasicReturn(string correlationId, int replyCode, string replyText)
        {
            CallbackInfo info;

            if (_callbacksDictionary.TryRemove(correlationId, out info))
            {
                info.SetResponse(null, new RpcCallException(RpcFailureReason.NotRouted, string.Format("Message not routed. Error code: {0}, reply: {1}", replyCode, replyText)));
            }
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
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

                var exceptionHeader =
                    properties.Headers.Where(pair => pair.Key == ExceptionHeader.WellknownName)
                        .Select(pair => pair.Value)
                        .FirstOrDefault();

                if (exceptionHeader != null)
                {
                    info.SetResponse(null, new RpcCallException(RpcFailureReason.HandlerError, exceptionHeader.ToString()));

                    return;
                }

                DataContractKey dataContractKey;
                object data;

                if (body.Length == 0 || info.ReplyType == null)
                {
                    // Reject without data
                    if (rejectHeader != null)
                    {
                        info.SetResponse(null, new RpcCallException(RpcFailureReason.Reject));

                        return;
                    }

                    // Void reply or sender not interested in reply data, but only interested to be notified that work is done
                    dataContractKey = DataContractKey.Void;
                    data = null;
                }
                else
                {
                    dataContractKey = properties.GetDataContractKey();

                    if (!_serializers.ContainsKey(properties.ContentType))
                    {
                        info.SetResponse(null,
                            new RpcCallException(RpcFailureReason.SerializationError,
                                string.Format("Unsupported content type {0}", properties.ContentType)));

                        return;
                    }

                    try
                    {
                        ISerializer serializer = _serializers[properties.ContentType];

                        if (dataContractKey.Equals(DataContractKey.BinaryBlob))
                        {
                            data = body;
                        }
                        else
                        {
                            data = serializer.Deserialize(info.ReplyType, body.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        info.SetResponse(null, new RpcCallException(RpcFailureReason.SerializationError, ex));

                        return;
                    }
                }

                // Reject with data
                if (rejectHeader != null)
                {
                    info.SetResponse(null, new RpcCallException(RpcFailureReason.Reject, data));

                    return;
                }

                RawBusMessage message = _messageHelper.ConstructMessage(dataContractKey, properties, data);

                _trace.MessageArrived(_busId, message, ConsumerTags.FirstOrDefault());

                info.SetResponse(message, null);
            }
        }

        public override void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
            _callbacksDictionary.Clear();
        }
    }
}