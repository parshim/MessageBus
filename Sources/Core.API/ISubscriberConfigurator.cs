using System.ServiceModel.Channels;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Interface to configure the requested subscriber
    /// </summary>
    public interface ISubscriberConfigurator
    {
        /// <summary>
        /// Specify buffer manager to be used for message transfer
        /// </summary>
        /// <param name="bufferManager"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseBufferManager(BufferManager bufferManager);

        /// <summary>
        /// Specify error subscriber interface to redirect subscriber errors to
        /// </summary>
        /// <param name="errorSubscriber"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber);

        /// <summary>
        /// Specify durable queue name to listen for the messages
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseDurableQueue(string queueName);
    }
}