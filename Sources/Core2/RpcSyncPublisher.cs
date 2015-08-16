using System;
using System.Threading;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RpcSyncPublisher : RpcPublisherBase, IRpcPublisher
    {
        public RpcSyncPublisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper, IRpcConsumer consumer)
            : base(model, busId, configuration, messageHelper, sendHelper, consumer)
        {

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

        public void Send<TData, TReplyData>(BusMessage<TData> busMessage, TimeSpan timeOut, Action<BusMessage<TReplyData>> onReply)
        {
            string id = GenerateCorrelationId();

            _consumer.RegisterCallback(id, typeof (TReplyData), timeOut, (replyRawMessage, exception) =>
            {
                if (exception == null && replyRawMessage != null)
                {
                    BusMessage<TReplyData> replyMessage = CreateBusMessage<TReplyData>(replyRawMessage);

                    onReply(replyMessage);
                }
            });

            SendMessage(busMessage, id);
        }

        public void Send<TData, TReplyData>(TData data, TimeSpan timeOut, Action<TReplyData> onReply)
        {
            Send<TData, TReplyData>(new BusMessage<TData> { Data = data }, timeOut, message => onReply(message.Data));
        }

        public BusMessage<TReplyData> Send<TData, TReplyData>(BusMessage<TData> busMessage, TimeSpan timeOut)
        {
            RawBusMessage replyMessage = SendAndWaitForReply(busMessage, timeOut, typeof(TReplyData));

            return CreateBusMessage<TReplyData>(replyMessage);
        }

        private RawBusMessage SendAndWaitForReply<TData>(BusMessage<TData> busMessage, TimeSpan timeOut, Type replyType)
        {
            string id = GenerateCorrelationId();

            RawBusMessage replyMessage = null;
            Exception exception = null;

            WaitHandle handle = _consumer.RegisterCallback(id, replyType, timeOut, (r, ex) =>
            {
                replyMessage = r;
                exception = ex;
            });

            SendMessage(busMessage, id);

            handle.WaitOne();
            
            if (exception != null)
            {
                throw exception;
            }

            return replyMessage;
        }
    }

    

}