namespace MessageBus.Core.API
{
    /// <summary>
    /// Bus level configurations
    /// </summary>
    public interface IBusConfigurator
    {
        /// <summary>
        /// Set custom bus id
        /// </summary>
        /// <param name="busId"></param>
        /// <returns></returns>
        IBusConfigurator SetBusId(string busId);

        /// <summary>
        /// Specify broker connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IBusConfigurator UseConnectionString(string connectionString);

        /// <summary>
        /// Specify error subscriber interface to redirect subscriber errors to.
        /// </summary>
        /// <param name="errorSubscriber"></param>
        /// <returns></returns>
        IBusConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber);

        /// <summary>
        /// Specify callback for messages which failed to be delivered.
        /// </summary>
        /// <param name="errorHandler">Callback to be called upon successful message delivery.</param>
        /// <returns></returns>
        IBusConfigurator UseErrorHandler(IPublishingErrorHandler errorHandler);
    }
}