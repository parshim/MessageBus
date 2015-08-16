using System;
using System.Threading.Tasks;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class RpcAsyncPublisher : RpcPublisherBase, IRpcAsyncPublisher
    {
        public RpcAsyncPublisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper, IRpcConsumer consumer)
            : base(model, busId, configuration, messageHelper, sendHelper, consumer)
        {
        }

        public Task Send<TData>(TData data, TimeSpan timeOut)
        {
            return Send(new BusMessage<TData> {Data = data}, timeOut);
        }

        public Task<TReplyData> Send<TData, TReplyData>(TData data, TimeSpan timeOut)
        {
            return SendAndCreateTask(new BusMessage<TData> { Data = data }, timeOut, m =>
            {
                BusMessage<TReplyData> replyMessage = CreateBusMessage<TReplyData>(m);

                return replyMessage.Data;
            });
        }
        
        public Task Send<TData>(BusMessage<TData> message, TimeSpan timeOut)
        {
            return SendAndCreateTask<TData, object>(message, timeOut, m => null);
        }

        public Task<BusMessage<TReplyData>> Send<TData, TReplyData>(BusMessage<TData> message, TimeSpan timeOut)
        {
            return SendAndCreateTask(message, timeOut, CreateBusMessage<TReplyData>);
        }

        private Task<TReplyData> SendAndCreateTask<TData, TReplyData>(BusMessage<TData> message, TimeSpan timeOut, Func<RawBusMessage, TReplyData> createReply)
        {
            string id = GenerateCorrelationId();

            var tcs = new TaskCompletionSource<TReplyData>();

            _consumer.RegisterCallback(id, typeof(TReplyData), timeOut, (r, ex) =>
            {
                if (ex != null)
                {
                    tcs.TrySetException(ex);
                }
                else
                {
                    TReplyData replyMessage = createReply(r);

                    tcs.TrySetResult(replyMessage);
                }
            });

            SendMessage(message, id);

            return tcs.Task;
        }

    }
}