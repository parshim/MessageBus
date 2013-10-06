namespace MessageBus.Core.API
{
    /// <summary>
    /// Message bus interface provides a way to publishers messages and subscriber for messages.
    /// All publishers and subsribers will be associated with same bus runtime.
    /// </summary>
    /// <example>
    /// IBus bus = new SomeBus()
    /// 
    /// using (bus)
    /// {
    ///     using (IPublisher publisher = bus.CreatePublisher())
    ///     {
    ///         publisher.Send(new MyData { Id = 5, Name = "hello" } );
    ///     }
    /// 
    ///     using (ISubscriber subscriber = bus.CreateSubscriber())
    ///     {
    ///         subscriber.Subscribe(d => Console.WriteLine(d.Address));
    ///     } // Once disposed no data will be consumed any more
    /// }
    /// </example>
    public interface IBus
    {
        /// <summary>
        /// Bus client name which uniquely identifies publishers and subscribers created within this instance
        /// </summary>
        string BusId { get; }

        /// <summary>
        /// Creates publisher session. It is recomended to open new session every time there is a need to send messages.
        /// </summary>
        /// <remarks>Publisher implementation should support transactions. Thus every time there is a need to send multiple messages, which are logically connected, it should be done inside transactio scope.</remarks>
        /// <returns>Publisher instance</returns>
        IPublisher CreatePublisher();

        /// <summary>
        /// Creates subscriber. Subscriber implementation should provide ordered message delivery, i.e. preserve message dispatching order.
        /// </summary>
        /// <remarks>
        /// To logicaly separate processing of different message types, separate subscriber instances should be created.
        /// </remarks>
        /// <returns>
        /// Subscriber instance.
        /// </returns>
        /// <exception cref="NoIncomingConnectionAcceptedException">No incoming connection were accepted.</exception>
        ISubscriber CreateSubscriber();
    }
}
