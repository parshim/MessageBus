using System;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace MessageBus.Binding.ZeroMQ
{
    public class ZMQRequestContext : RequestContext
    {
        private readonly Message _requestMessage;
        private readonly ZMQChannelBase _channel;

        public ZMQRequestContext(Message requestMessage, ZMQChannelBase channel)
        {
            _requestMessage = requestMessage;
            _channel = channel;
        }

        public override void Abort()
        {
            Close();
        }

        public override void Close()
        {
            _requestMessage.Close();
        }

        public override void Close(TimeSpan timeout)
        {
            Close();
        }

        public override void Reply(Message message)
        {
            _channel.Send(message);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            _channel.Send(message);
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return BeginReply(message, TimeSpan.MaxValue, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(s => Reply(message, timeout), state).ContinueWith(task => callback(task));
        }

        public override void EndReply(IAsyncResult result)
        {
            Task.Factory.FromAsync(result, asyncResult => { }).Wait();
        }

        public override Message RequestMessage
        {
            get { return _requestMessage; }
        }
    }
}