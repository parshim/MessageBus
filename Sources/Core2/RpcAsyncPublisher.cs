using System;
using System.Threading.Tasks;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RpcAsyncPublisher : RpcPublisherBase, IRpcAsyncPublisher
    {
        public RpcAsyncPublisher(IModel model, string busId, RpcPublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper, IRpcConsumer consumer)
            : base(model, busId, configuration, messageHelper, sendHelper, consumer)
        {
        }

        public Task Send<TData>(TData data, TimeSpan timeOut, bool persistant)
        {
            return Send(new BusMessage<TData> { Data = data }, timeOut, persistant);
        }

        public Task<TReplyData> Send<TData, TReplyData>(TData data, TimeSpan timeOut, bool persistant)
        {
            return SendAndCreateTask(new BusMessage<TData> { Data = data }, timeOut, m =>
            {
                BusMessage<TReplyData> replyMessage = CreateBusMessage<TReplyData>(m);

                return replyMessage.Data;
            }, persistant);
        }

        public Task Send<TData>(BusMessage<TData> message, TimeSpan timeOut, bool persistant)
        {
            return SendAndCreateTask<TData, object>(message, timeOut, m => null, persistant);
        }

        public Task<BusMessage<TReplyData>> Send<TData, TReplyData>(BusMessage<TData> message, TimeSpan timeOut, bool persistant)
        {
            return SendAndCreateTask(message, timeOut, CreateBusMessage<TReplyData>, persistant);
        }

        private Task<TReplyData> SendAndCreateTask<TData, TReplyData>(BusMessage<TData> message, TimeSpan timeOut, Func<RawBusMessage, TReplyData> createReply, bool persistant)
        {
            string id = NewMiniGuid();

            var tcs = new TaskCompletionSource<TReplyData>();

            _consumer.RegisterCallback(id, typeof(TReplyData), timeOut, (r, ex) =>
            {
                if (ex != null)
                {
                    tcs.SetException(ex);
                }
                else
                {
                    TReplyData replyMessage = createReply(r);

                    tcs.SetResult(replyMessage);
                }
            });

            SendMessage(message, id, persistant);

            return tcs.Task;
        }

    }
}