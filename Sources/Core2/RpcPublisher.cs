using System;
using System.Threading;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RpcPublisher : PublisherBase, IRpcPublisher
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

        public bool Send<TData, TReplyData>(TData data, TimeSpan timeOut, out TReplyData reply)
        {
            BusMessage<TReplyData> replyMessage;
            
            bool result = Send(new BusMessage<TData>
            {
                Data = data
            }, timeOut, out replyMessage);

            reply = replyMessage != null ? replyMessage.Data : default(TReplyData);

            return result;
        }

        public bool Send<TData, TReplyData>(BusMessage<TData> busMessage, TimeSpan timeOut, out BusMessage<TReplyData> busReplyMessage)
        {
            string id = GenerateCorrelationId();

            using (ManualResetEvent ev = new ManualResetEvent(false))
            {
                RawBusMessage replyMessage = null;
                Exception exception = null;

                _consumer.RegisterCallback(id, typeof(TReplyData), (r, ex) =>
                {
                    replyMessage = r;
                    exception = ex;

                    try
                    {
                        ev.Set();
                    }
                    catch { }
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
                    busReplyMessage = null;

                    return false;
                }

                if (exception != null)
                {
                    throw exception;
                }

                busReplyMessage = new BusMessage<TReplyData>
                {
                    BusId = replyMessage.BusId,
                    Sent = replyMessage.Sent,
                    Data = (TReplyData)replyMessage.Data
                };

                foreach (var header in replyMessage.Headers)
                {
                    busMessage.Headers.Add(header);
                }

                return true;
            }
        }

        private static string GenerateCorrelationId()
        {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return id.TrimEnd('=');
        }
    }

    

}