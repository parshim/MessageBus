using System;
using System.Threading;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RpcPublisher : Publisher, IRpcPublisher
    {
        const string Queue = "amq.rabbitmq.reply-to";

        private readonly IRpcConsumer _consumer;

        public RpcPublisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper, IRpcConsumer consumer) : base(model, busId, configuration, messageHelper, sendHelper)
        {
            _consumer = consumer;

            model.BasicConsume(Queue, true, consumer);
        }

        protected override void OnMessageReturn(int replyCode, string replyText, RawBusMessage message)
        {
            _consumer.HandleBasicReturn(message.CorrelationId, replyCode, replyText);
        }

        public TReplyData Send<TData, TReplyData>(TData data, TimeSpan timeOut)
        {
            return Send<TData, TReplyData>(new BusMessage<TData> { Data = data }, timeOut).Data;
        }

        public void Send<TData>(TData data, TimeSpan timeOut)
        {
            Send(new BusMessage<TData> {Data = data}, timeOut);
        }

        public void Send<TData>(BusMessage<TData> busMessage, TimeSpan timeOut)
        {
            SendAndWaitForReply(busMessage, timeOut, null);
        }

        public BusMessage<TReplyData> Send<TData, TReplyData>(BusMessage<TData> busMessage, TimeSpan timeOut)
        {
            RawBusMessage replyMessage = SendAndWaitForReply(busMessage, timeOut, typeof(TReplyData));

            BusMessage<TReplyData> busReplyMessage = new BusMessage<TReplyData>
            {
                BusId = replyMessage.BusId,
                Sent = replyMessage.Sent,
                Data = (TReplyData) replyMessage.Data
            };

            foreach (var header in replyMessage.Headers)
            {
                busMessage.Headers.Add(header);
            }

            return busReplyMessage;
        }

        private RawBusMessage SendAndWaitForReply<TData>(BusMessage<TData> busMessage, TimeSpan timeOut, Type replyType)
        {
            string id = GenerateCorrelationId();

            using (ManualResetEvent ev = new ManualResetEvent(false))
            {
                RawBusMessage replyMessage = null;
                Exception exception = null;

                _consumer.RegisterCallback(id, replyType, (r, ex) =>
                {
                    replyMessage = r;
                    exception = ex;

                    try
                    {
                        ev.Set();
                    }
                    catch
                    {
                    }
                });

                RawBusMessage rawBusMessage = new RawBusMessage
                {
                    Data = busMessage.Data
                };

                foreach (var header in busMessage.Headers)
                {
                    rawBusMessage.Headers.Add(header);
                }

                _sendHelper.Send(new SendParams
                {
                    BusId = _busId,
                    Model = _model,
                    BusMessage = rawBusMessage,
                    CorrelationId = id,
                    Serializer = _configuration.Serializer,
                    Exchange = _configuration.Exchange,
                    MandatoryDelivery = true,
                    PersistentDelivery = _configuration.PersistentDelivery,
                    RoutingKey = _configuration.RoutingKey,
                    ReplyTo = Queue
                });

                bool waitOne = ev.WaitOne(timeOut);

                if (!waitOne)
                {
                    throw new RpcCallException(RpcFailureReason.TimeOut);
                }

                if (exception != null)
                {
                    throw exception;
                }

                return replyMessage;
            }
        }
        
        private static string GenerateCorrelationId()
        {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return id.TrimEnd('=');
        }
    }

    

}