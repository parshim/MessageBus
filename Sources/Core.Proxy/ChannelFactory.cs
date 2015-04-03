using System;
using Castle.DynamicProxy;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public class ChannelFactory<T> : IChannelFactory<T> where T : class
    {
        private readonly ProxyGenerator _generator = new ProxyGenerator();
        private readonly IMessageFactory _messageFactory;

        private readonly IPublisher _publisher;

        public ChannelFactory(IBus bus, Action<IPublisherConfigurator> configurator = null)
        {
            string ns = typeof(T).GetMessageNamespace();

            _messageFactory = new MessageFactory(ns);

            _publisher = bus.CreatePublisher(configurator);
        }

        public T CreateChannel(params BusHeader[] headers) 
        {
            return _generator.CreateInterfaceProxyWithoutTarget<T>(new PublishInterceptor(_messageFactory, _publisher, headers));
        }

        public void Dispose()
        {
            _publisher.Dispose();
        }
    }
}