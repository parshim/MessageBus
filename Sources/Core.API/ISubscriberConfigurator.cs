using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Interface to configure the requested subscriber
    /// </summary>
    public interface ISubscriberConfigurator
    {
        /// <summary>
        /// Specify buffer manager to be used for message transfer.
        /// </summary>
        /// <param name="bufferManager"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseBufferManager(BufferManager bufferManager);

        /// <summary>
        /// Specify error subscriber interface to redirect subscriber errors to.
        /// </summary>
        /// <param name="errorSubscriber"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber);

        /// <summary>
        /// Specify durable queue name to listen for the messages.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseDurableQueue(string queueName);

        /// <summary>
        /// Specify transactional delivery of the messages. If exception will be thrown on subscribed action message will be returned to the queue.
        /// </summary>
        /// <returns></returns>
        ISubscriberConfigurator UseTransactionalDelivery();

        /// <summary>
        /// Specify transactional delivery of the messages. If exception will be thrown on subscribed action message will be returned to the queue.
        /// </summary>
        /// <param name="exceptionFilter">Exception filter</param>
        /// <returns></returns>
        ISubscriberConfigurator UseTransactionalDelivery(IExceptionFilter exceptionFilter);
        
        /// <summary>
        /// If set, messages published within bus instance will be received and processed by subscriber.
        /// <remarks>By default self-published messages are ignored.</remarks>
        /// </summary>
        ISubscriberConfigurator SetReceiveSelfPublish(bool receive = true);

        /// <summary>
        /// If set, even for messages with reply-to filed reply will not be sent. 
        /// <remarks>This flag is usefull for scenarious when synchronous reply is required while there are number of different subscribers for the message. 
        /// In such case every subscriber will issue a reply and RPC publisher will get first one. Sometimes system logic requires to get reply from specific subscriber, 
        /// while other subscribers will process message silently. For silent subscribers SetNeverReply should be called during configuration.</remarks>
        /// </summary>
        ISubscriberConfigurator SetNeverReply(bool neverReply = true);

        /// <summary>
        /// Specifies concurency level of the subscriber. Level grater then one may cause lose of message order between processing threads.
        /// <remarks>By default concurency level is 1.</remarks>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        ISubscriberConfigurator SetConcurencyLevel(int level);

        /// <summary>
        /// Specifies task scheduler that will be used to dispatch received messages to subscriber.
        /// </summary>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        ISubscriberConfigurator UseTaskScheduler(TaskScheduler scheduler);

        /// <summary>
        /// Specify custom serializer for received messages.
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        ISubscriberConfigurator AddCustomSerializer(ISerializer serializer);

        /// <summary>
        /// Specify exchange messages subscriptions should be bound to.
        /// </summary>
        ISubscriberConfigurator SetExchange(string exchange);

        /// <summary>
        /// Specify routing key for messages bindings.
        /// </summary>
        ISubscriberConfigurator SetRoutingKey(string routingKey);
    }
}