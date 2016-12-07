﻿namespace MessageBus.Core.API
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
        /// <param name="nameOrConnectionString"></param>
        /// <returns></returns>
        IBusConfigurator UseConnectionString(string nameOrConnectionString);

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
        
        /// <summary>
        /// Specify trace to log every sent and arrived message in to.
        /// </summary>
        /// <returns></returns>
        IBusConfigurator UseTrace(ITrace trace);

        /// <summary>
        /// If set, messages published within bus instance will be received and processed by subscriber.
        /// <remarks>By default self-published messages are ignored.</remarks>
        /// </summary>
        IBusConfigurator SetReceiveSelfPublish();

        /// <summary>
        /// Disable fast reply mechanism introduced in RabbitMQ. Publisher will create dedicated  auto-delete queue to consume reply messages. 
        /// If replyExchange parameter is different from default exchange (empty string) use IPublisherConfigurator.SetReplyTo method to specify routing key to bind queue to reply exchange.
        /// </summary>
        /// <returns></returns>
        IBusConfigurator DisableFastReply();

        /// <summary>
        /// Set exchange name for reply messages. It can be used if fast reply is disabled and reply queue should be bounded to specified exchange. 
        /// By default reply queue will get messages from default exchange. 
        /// </summary>
        /// <param name="replyExchange"></param>
        /// <returns></returns>
        IBusConfigurator SetReplyExchange(string replyExchange);
    }
}