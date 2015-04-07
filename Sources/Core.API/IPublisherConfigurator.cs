using System.ServiceModel.Channels;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Interface to configure the requested publisher
    /// </summary>
    public interface IPublisherConfigurator
    {
        /// <summary>
        /// Specify buffer manager to be used for message transfer
        /// </summary>
        /// <param name="bufferManager"></param>
        /// <returns></returns>
        IPublisherConfigurator UseBufferManager(BufferManager bufferManager);

        /// <summary>
        /// Specify callback for messages which failed to be delivered
        /// </summary>
        /// <param name="errorHandler">Callback to be called upon successful message delivery</param>
        /// <returns></returns>
        IPublisherConfigurator UseErrorHandler(IPublishingErrorHandler errorHandler);

        /// <summary>
        /// Specify that message should be sent to at least one subscriber.
        /// Otherwise error handler will be notified about undeliverable message.
        /// Use UseErrorHandler to register error handler implementation
        /// </summary>
        IPublisherConfigurator SetMandatoryDelivery();

        /// <summary>
        /// Specify that message should be persistent on queue.
        /// </summary>
        IPublisherConfigurator SetPersistentDelivery();

        /// <summary>
        /// Specify that published messages should be serialized as Soap messages
        /// </summary>
        IPublisherConfigurator UseSoapSerializer();

        /// <summary>
        /// Specify custom serializer for published messages
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        IPublisherConfigurator UseCustomSerializer(ISerializer serializer);
    }
}