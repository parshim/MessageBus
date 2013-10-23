namespace MessageBus.Core.API
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPublishingErrorHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="text"></param>
        /// <param name="message"></param>
        void DeliveryFailed(int errorCode, string text, RawBusMessage message);
    }
}