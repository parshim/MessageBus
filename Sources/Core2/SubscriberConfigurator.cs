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
        private IExceptionFilter _exceptionFilter = new NullExceptionFilter();
        private string _queueName = "";
        private bool _receiveSelfPublish;
        private bool _neverReply;
        private string _replyExchange;
        private bool _transactionalDelivery;
        private string _exchange;
        private string _routingKey = "";

        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        public SubscriberConfigurator(string exchange, string replyExchange, IErrorSubscriber errorSubscriber, bool receiveSelfPublish)
        {
            _exchange = exchange;
            _errorSubscriber = errorSubscriber;
            _receiveSelfPublish = receiveSelfPublish;
            _replyExchange = replyExchange;

            ISerializer jsonSerializer = new JsonSerializer();
            ISerializer soapSerializer = new SoapSerializer();

            _serializers.Add(jsonSerializer.ContentType, jsonSerializer);
            _serializers.Add(soapSerializer.ContentType, soapSerializer);
        }

        public string QueueName
        {
            get { return _queueName; }
        }
        public string Exchange
        {
            get { return _exchange; }
        }
        public string RoutingKey
        {
            get { return _routingKey; }
        }

        public bool TransactionalDelivery
        {
            get { return _transactionalDelivery; }
        }

        public IExceptionFilter ExceptionFilter
        {
            get { return _exceptionFilter; }
        }

        public BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        public IErrorSubscriber ErrorSubscriber
        {
            get { return _errorSubscriber; }
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

        public bool NeverReply
        {
            get
            {
                return _neverReply;
            }
        }

        public string ReplyExchange
        {
            get
            {
                return _replyExchange;
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

        public ISubscriberConfigurator UseTransactionalDelivery(IExceptionFilter exceptionFilter)
        {
            _transactionalDelivery = true;
            _exceptionFilter = exceptionFilter;

            return this;
        }

        public ISubscriberConfigurator SetReceiveSelfPublish(bool receive)
        {
            _receiveSelfPublish = receive;

            return this;
        }

        public ISubscriberConfigurator SetNeverReply(bool neverReply = true)
        {
            _neverReply = neverReply;

            return this;
        }

        public ISubscriberConfigurator SetReplyExchange(string replyExchange)
        {
            _replyExchange = replyExchange;

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

        public ISubscriberConfigurator SetExchange(string exchange)
        {
            _exchange = exchange;

            return this;
        }

        public ISubscriberConfigurator SetRoutingKey(string routingKey)
        {
            _routingKey = routingKey;

            return this;
        }
    }
}