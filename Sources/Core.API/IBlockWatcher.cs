namespace MessageBus.Core.API
{
    /// <summary>
    /// Connection block whatcher
    /// </summary>
    public interface IBlockWatcher
    {
        /// <summary>
        /// Notifies upon connection blocking. Messages cannont be published via blocked connections
        /// </summary>
        /// <param name="reason">Block reason</param>
        void ConnectionBlocked(string reason);

        /// <summary>
        /// Notifies upon connection unblocking.
        /// </summary>
        void ConnectionUnblocked();
    }
}