namespace MessageBus.Core.API
{
    public interface ITransactionalPublisher : IPublisher
    {

        /// <summary>
        /// Commits message delivery
        /// </summary>
        void Commit();

        /// <summary>
        /// Rallback message delivery
        /// </summary>
        void Rollback();
    }
}