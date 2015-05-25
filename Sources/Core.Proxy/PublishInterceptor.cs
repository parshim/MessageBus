using System.Collections.Generic;
using Castle.DynamicProxy;

using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public class PublishInterceptor : IInterceptor
    {
        private readonly IMessageFactory _messageFactory;

        private readonly IPublisher _publisher;

        private readonly IHeadersProvider _headersProvider;

        public PublishInterceptor(IMessageFactory messageFactory, IPublisher publisher, IHeadersProvider headersProvider)
        {
            _messageFactory = messageFactory;
            _publisher = publisher;
            _headersProvider = headersProvider;
        }

        public void Intercept(IInvocation invocation)
        {
            object data = _messageFactory.CreateMessage(invocation.Method, invocation.Arguments);

            RawBusMessage message = new RawBusMessage
            {
                Data = data,
            };

            IEnumerable<BusHeader> headers = _headersProvider.GetMessageHeaders();

            foreach (BusHeader header in headers)
            {
                message.Headers.Add(header);
            }

            _publisher.Send(message);
        }
    }
}