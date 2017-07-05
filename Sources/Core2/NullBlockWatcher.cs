using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class NullBlockWatcher : IBlockWatcher
    {
        public void ConnectionBlocked(string reason)
        {
            
        }

        public void ConnectionUnblocked()
        {

        }
    }
}