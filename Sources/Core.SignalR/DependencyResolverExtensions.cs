using System;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

namespace Core.SignalR
{
    public static class DependencyResolverExtensions
    {
        /// <summary>
        /// Use RabbitMQ as the messaging backplane for scaling out of ASP.NET SignalR applications in a web farm.
        /// </summary>
        /// <param name="resolver">The dependency resolver</param>
        /// <param name="configuration">The RabbitMQ scale-out configuration options.</param> 
        /// <returns>The dependency resolver.</returns>
        public static IDependencyResolver UseRabbit(this IDependencyResolver resolver, RabbitScaleoutConfiguration configuration)
        {
            var bus = new Lazy<RabbitMessageBus>(() => new RabbitMessageBus(resolver, configuration));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }
    }
}
