using System;
using System.Threading;
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

        public Task Send<TData>(TData data, bool persistant, CancellationToken cancellationToken)
        {
            return Send(new BusMessage<TData> { Data = data }, persistant, cancellationToken);
        }

        public Task<TReplyData> Send<TData, TReplyData>(TData data, bool persistant, CancellationToken cancellationToken)
        {
            return SendAndCreateTask(new BusMessage<TData> { Data = data }, cancellationToken, m =>
            {
                BusMessage<TReplyData> replyMessage = CreateBusMessage<TReplyData>(m);

                return replyMessage.Data;
            }, persistant);
        }

        public Task Send<TData>(BusMessage<TData> message, bool persistant, CancellationToken cancellationToken)
        {
            return SendAndCreateTask<TData, object>(message, cancellationToken, m => null, persistant);
        }

        public Task<BusMessage<TReplyData>> Send<TData, TReplyData>(BusMessage<TData> message, bool persistant, CancellationToken cancellationToken)
        {
            return SendAndCreateTask(message, cancellationToken, CreateBusMessage<TReplyData>, persistant);
        }

        private Task<TReplyData> SendAndCreateTask<TData, TReplyData>(BusMessage<TData> message, CancellationToken cancellationToken, Func<RawBusMessage, TReplyData> createReply, bool persistant)
        {
            string id = NewMiniGuid();

            TaskCompletionSource<TReplyData> taskCompletionSource = new TaskCompletionSource<TReplyData>();

            var registration = cancellationToken.Register(() =>
            {
                if (taskCompletionSource.TrySetCanceled())
                {
                    _consumer.RemoveCallback(id);
                }
            });

            _consumer.RegisterCallback(id, typeof(TReplyData), (r, ex) =>
            {
                if (ex != null)
                {
                    taskCompletionSource.TrySetException(ex);
                }
                else
                {
                    var replyMessage = createReply(r);

                    taskCompletionSource.TrySetResult(replyMessage);
                }

                registration.Dispose();
            });

            SendMessage(message, id, persistant);

            return taskCompletionSource.Task;
        }

    }
}