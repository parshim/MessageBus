using System;
using System.Threading;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RpcSyncPublisher : RpcPublisherBase, IRpcPublisher
    {
        public RpcSyncPublisher(IModel model, string busId, RpcPublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper, IRpcConsumer consumer)
            : base(model, busId, configuration, messageHelper, sendHelper, consumer)
        {

        }

        public TReplyData Send<TData, TReplyData>(TData data, TimeSpan timeOut, bool persistant)
        {
            return Send<TData, TReplyData>(new BusMessage<TData> { Data = data }, timeOut, persistant).Data;
        }

        public void Send<TData>(TData data, TimeSpan timeOut, bool persistant)
        {
            Send(new BusMessage<TData> {Data = data}, timeOut, persistant);
        }

        public void Send<TData>(BusMessage<TData> busMessage, TimeSpan timeOut, bool persistant)
        {
            SendAndWaitForReply(busMessage, timeOut, null, persistant);
        }

        public void Send<TData, TReplyData>(BusMessage<TData> busMessage, TimeSpan timeOut, Action<BusMessage<TReplyData>> onReply, bool persistant)
        {
            string id = NewMiniGuid();

            _consumer.RegisterCallback(id, typeof (TReplyData), (replyRawMessage, exception) =>
            {
                if (exception == null && replyRawMessage != null)
                {
                    BusMessage<TReplyData> replyMessage = CreateBusMessage<TReplyData>(replyRawMessage);

                    onReply(replyMessage);
                }
            });

            SendMessage(busMessage, id, persistant);
        }

        public void Send<TData, TReplyData>(TData data, TimeSpan timeOut, Action<TReplyData> onReply, bool persistant)
        {
            Send<TData, TReplyData>(new BusMessage<TData> { Data = data }, timeOut, message => onReply(message.Data), persistant);
        }

        public BusMessage<TReplyData> Send<TData, TReplyData>(BusMessage<TData> busMessage, TimeSpan timeOut, bool persistant)
        {
            RawBusMessage replyMessage = SendAndWaitForReply(busMessage, timeOut, typeof(TReplyData), persistant);

            return CreateBusMessage<TReplyData>(replyMessage);
        }

        private RawBusMessage SendAndWaitForReply<TData>(BusMessage<TData> busMessage, TimeSpan timeOut, Type replyType, bool persistant)
        {
            string id = NewMiniGuid();

            RawBusMessage replyMessage = null;
            Exception exception = null;

            WaitHandle handle = _consumer.RegisterCallback(id, replyType, (r, ex) =>
            {
                replyMessage = r;
                exception = ex;
            });

            try
            {
                SendMessage(busMessage, id, persistant);

                if (!handle.WaitOne(timeOut))
                {
                    exception = new RpcCallException(RpcFailureReason.TimeOut);
                }
            }
            finally
            {
                _consumer.RemoveCallback(id);
            }
            
            if (exception != null)
            {
                throw exception;
            }

            return replyMessage;
        }
    }

    

}