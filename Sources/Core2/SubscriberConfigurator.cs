using System.Collections.Generic;
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
        private bool _transactionalDelivery;

        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();
        
        public SubscriberConfigurator()
        {
            ISerializer jsonSerializer = new JsonSerializer();
            ISerializer soapSerializer = new SoapSerializer();

            _serializers.Add(jsonSerializer.ContentType, jsonSerializer);
            _serializers.Add(soapSerializer.ContentType, soapSerializer);
        }

        public string QueueName
        {
            get { return _queueName; }
        }

        public bool TransactionalDelivery
        {
            get { return _transactionalDelivery; }
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

        public Dictionary<string, ISerializer> Serializers
        {
            get { return _serializers; }
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

        public ISubscriberConfigurator UseTransactionalDelivery()
        {
            _transactionalDelivery = true;

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

        public ISubscriberConfigurator AddCustomSerializer(ISerializer serializer)
        {
            _serializers.Add(serializer.ContentType, serializer);

            return this;
        }
    }
}