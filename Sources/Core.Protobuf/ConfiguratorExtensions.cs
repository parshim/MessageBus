using MessageBus.Core.API;

namespace MessageBus.Core.Protobuf
{
    public static class ConfiguratorExtensions
    {
        public static IPublisherConfigurator UseProtobufSerializer(this IPublisherConfigurator configurator)
        {
            return configurator.UseCustomSerializer(new ProtobufSerializer());
        }

        public static ISubscriberConfigurator AddProtobufSerializer(this ISubscriberConfigurator configurator)
        {
            return configurator.AddCustomSerializer(new ProtobufSerializer());
        }
    }
}