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
        /// 
        /// </summary>
        /// <param name="errorSubscriber"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber);
    }
}