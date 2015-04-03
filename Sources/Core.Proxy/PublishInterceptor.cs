using Castle.DynamicProxy;

using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public class PublishInterceptor : IInterceptor
    {
        private readonly IMessageFactory _messageFactory;

        private readonly IPublisher _publisher;

        private readonly BusHeader[] _headers;

        public PublishInterceptor(IMessageFactory messageFactory, IPublisher publisher, BusHeader[] headers)
        {
            _messageFactory = messageFactory;
            _publisher = publisher;
            _headers = headers;
        }

        public void Intercept(IInvocation invocation)
        {
            object data = _messageFactory.CreateMessage(invocation.Method, invocation.Arguments);

            RawBusMessage message = new RawBusMessage
            {
                Data = data,
            };

            foreach (BusHeader header in _headers)
            {
                message.Headers.Add(header);
            }

            _publisher.Send(message);
        }
    }
}