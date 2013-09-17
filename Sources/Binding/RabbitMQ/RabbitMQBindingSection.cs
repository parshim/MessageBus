using System.ServiceModel.Configuration;

namespace MessageBus.Binding.RabbitMQ
{  
    /// <summary>
    /// Allows the RabbitMQBinding to be declarativley configured
    /// </summary>
    public sealed class RabbitMQBindingSection : StandardBindingCollectionElement<RabbitMQBinding, RabbitMQBindingConfigurationElement>
    {
    }
}
