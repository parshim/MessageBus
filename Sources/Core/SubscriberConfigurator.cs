using System.ServiceModel.Channels;
using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class SubscriberConfigurator : ISubscriberConfigurator
    {
        private BufferManager _bufferManager;
        private IErrorSubscriber _errorSubscriber;
        private string _queueName;

        public string QueueName
        {
            get { return _queueName; }
        }

        public BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        public IErrorSubscriber ErrorSubscriber
        {
            get { return _errorSubscriber ?? new NullErrorSubscriber(); }
        }

        public ISubscriberConfigurator UseBufferManager(BufferManager bufferManager)
        {
            _bufferManager = bufferManager;

            return this;
        }

        public ISubscriberConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber)
        {
            _errorSubscriber = errorSubscriber;

            return this;
        }

        public ISubscriberConfigurator UseDurableQueue(string queueName)
        {
            _queueName = queueName;

            return this;
        }

        public ISubscriberConfigurator SetReceiveSelfPublish()
        {
            throw new System.NotImplementedException();
        }

        public ISubscriberConfigurator SetConcurencyLevel(int level)
        {
            throw new System.NotImplementedException();
        }

        public ISubscriberConfigurator UseTaskScheduler(TaskScheduler scheduler)
        {
            throw new System.NotImplementedException();
        }
    }
}