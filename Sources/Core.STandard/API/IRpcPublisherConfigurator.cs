namespace MessageBus.Core.API
{
    public interface IRpcPublisherConfigurator : IPublisherConfigurator
    {
        /// <summary>
        /// Disable fast reply mechanism introduced in RabbitMQ. Publisher will create dedicated  auto-delete queue to consume reply messages. 
        /// If replyExchange parameter is different from default exchange (empty string) use IPublisherConfigurator.SetReplyTo method to specify routing key to bind queue to reply exchange.
        /// </summary>
        /// <returns></returns>
        IRpcPublisherConfigurator DisableFastReply();

        /// <summary>
        /// Set exchange name for reply messages. It can be used if fast reply is disabled and reply queue should be bounded to specified exchange. 
        /// By default reply queue will get messages from default exchange. 
        /// </summary>
        /// <param name="replyExchange"></param>
        /// <returns></returns>
        IRpcPublisherConfigurator SetReplyExchange(string replyExchange);
        
        /// <summary>
        /// Specify consumer tag.
        /// </summary>
        IRpcPublisherConfigurator SetConsumerTag(string consumerTag);
    }
}