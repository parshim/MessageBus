using System.ServiceModel.Channels;
using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class SubscriberConfigurator : ISubscriberConfigurator
    {
        private TaskScheduler _taskScheduler;
        private BufferManager _bufferManager;
        private IErrorSubscriber _errorSubscriber;
        private string _queueName = "";
        private bool _receiveSelfPublish;

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

        public TaskScheduler TaskScheduler
        {
            get { return _taskScheduler ?? new LimitedConcurrencyLevelTaskScheduler(1); }
        }

        public bool ReceiveSelfPublish
        {
            get
            {
                return _receiveSelfPublish;
            }
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
            _receiveSelfPublish = true;

            return this;
        }

        public ISubscriberConfigurator SetConcurencyLevel(int level)
        {
            _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(level);

            return this;
        }

        public ISubscriberConfigurator UseTaskScheduler(TaskScheduler scheduler)
        {
            _taskScheduler = scheduler;

            return this;
        }
    }
}