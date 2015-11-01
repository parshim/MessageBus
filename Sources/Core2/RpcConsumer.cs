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
        private readonly RegisteredWaitHandle _handle;

        private RawBusMessage _message;
        private Exception _exception;

        public CallbackInfo(Action<RawBusMessage, Exception> callback, Type replyType)
        {
            _callback = callback;
            _replyType = replyType;
        }

        public CallbackInfo(Action<RawBusMessage, Exception> callback, Type replyType, ManualResetEvent ev, RegisteredWaitHandle handle)
        {
            _callback = callback;
            _replyType = replyType;
            _ev = ev;
            _handle = handle;
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

        public RegisteredWaitHandle RegisteredHandle
        {
            get { return _handle; }
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

        public RpcConsumer(IModel model, IMessageHelper messageHelper, Dictionary<string, ISerializer> serializers)
            : base(model)
        {
            _serializers = serializers;
            _messageHelper = messageHelper;
        }

        public WaitHandle RegisterCallback(string correlationId, Type replyType, TimeSpan timeOut, Action<RawBusMessage, Exception> callback)
        {
            CallbackInfo callbackInfo = _callbacksDictionary.GetOrAdd(correlationId, id => CreateCallback(id, replyType, timeOut, callback));

            return callbackInfo.WaitHandle;
        }

        private CallbackInfo CreateCallback(string id, Type replyType, TimeSpan timeOut, Action<RawBusMessage, Exception> callback)
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            RegisteredWaitHandle handle = ThreadPool.RegisterWaitForSingleObject(ev, CallbackTimeout, id, timeOut, true);
            
            return new CallbackInfo(callback, replyType, ev, handle);
        }

        private void CallbackTimeout(object state, bool timedout)
        {
            string correlationId = (string) state;

            CallbackInfo info;

            if (_callbacksDictionary.TryRemove(correlationId, out info))
            {
                if (timedout)
                {
                    info.SetResponse(null, new RpcCallException(RpcFailureReason.TimeOut));
                }
            }
        }

        public void HandleBasicReturn(string correlationId, int replyCode, string replyText)
        {
            CallbackInfo info;

            if (_callbacksDictionary.TryRemove(correlationId, out info))
            {
                info.SetResponse(null, new RpcCallException(RpcFailureReason.NotRouted, string.Format("Message not routed. Error code: {0}, reply: {1}", replyCode, replyText)));

                info.RegisteredHandle.Unregister(info.WaitHandle);
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
                    info.SetResponse(null, new RpcCallException(RpcFailureReason.Reject));

                    return;
                }

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

                        data = serializer.Deserialize(dataContractKey, info.ReplyType, body);
                    }
                    catch (Exception ex)
                    {
                        info.SetResponse(null, new RpcCallException(RpcFailureReason.SerializationError, ex));

                        return;
                    }
                }

                RawBusMessage message = _messageHelper.ConstructMessage(dataContractKey, properties, data);

                info.SetResponse(message, null);

                info.RegisteredHandle.Unregister(info.WaitHandle);
            }
        }

        public override void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
            _callbacksDictionary.Clear();
        }
    }
}