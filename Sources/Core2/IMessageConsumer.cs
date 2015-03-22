using RabbitMQ.Client;

namespace MessageBus.Core
{
    public interface IMessageConsumer : IBasicConsumer, IMessageRegistry
    {
        
    }
}