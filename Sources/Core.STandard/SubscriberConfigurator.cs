using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Core.API;
using Newtonsoft.Json;

namespace MessageBus.Core
{
    public class SubscriberConfigurator : ISubscriberConfigurator
    {
        private TaskScheduler _taskScheduler;
        private IErrorSubscriber _errorSubscriber;
        private ITrace _trace;
        private IExceptionFilter _exceptionFilter = new NullExceptionFilter();
        private string _queueName = string.Empty;
        private bool _receiveSelfPublish;
        private bool _neverReply;
        private string _replyExchange;
        private bool _transactionalDelivery;
        private ushort _prefetch;
        private string _exchange;
        private string _routingKey = string.Empty;
        private string _consumerTag = string.Empty;
        private bool _createBindings = true;
        private bool _autoCreate = true;
        private bool _durable = true;
        private bool _isExclusive = false;
        private sbyte _maxPriority;
        private readonly Func<bool> _blocked;

        private JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None
        };

        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        public SubscriberConfigurator(string exchange, string replyExchange, IErrorSubscriber errorSubscriber, bool receiveSelfPublish, ITrace trace, Func<bool> blocked)
        {
            _exchange = exchange;
            _errorSubscriber = errorSubscriber;
            _receiveSelfPublish = receiveSelfPublish;
            _trace = trace;
            _replyExchange = replyExchange;
            _blocked = blocked;
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

        public ushort Prefetch
        {
            get { return _prefetch; }
        }

        public IExceptionFilter ExceptionFilter
        {
            get { return _exceptionFilter; }
        }

        public IErrorSubscriber ErrorSubscriber
        {
            get { return _errorSubscriber; }
        }

        public ITrace Trace
        {
            get { return _trace; }
        }

        public TaskScheduler TaskScheduler
        {
            get { return _taskScheduler ?? new SyncTaskScheduler(); }
        }

        public Dictionary<string, ISerializer> Serializers
        {
            get
            {
                ISerializer jsonSerializer = new JsonSerializer(_settings);
                ISerializer xmlSerializer = new XmlSerializer();

                return new Dictionary<string, ISerializer>(_serializers)
                {
                    {jsonSerializer.ContentType, jsonSerializer},
                    {xmlSerializer.ContentType, xmlSerializer}
                };
            }
        }

        public bool ReceiveSelfPublish
        {
            get { return _receiveSelfPublish; }
        }

        public bool NeverReply
        {
            get { return _neverReply; }
        }

        public string ReplyExchange
        {
            get { return _replyExchange; }
        }

        public string ConsumerTag
        {
            get { return _consumerTag; }
        }

        public bool CreateBindings
        {
            get { return _createBindings; }
        }

        public bool Durable
        {
            get { return _durable; }
        }

        public bool AutoCreate
        {
            get { return _autoCreate; }
        }

        public bool IsExclusive
        {
            get { return _isExclusive; }
        }

        public sbyte MaxPriority
        {
            get { return _maxPriority; }
        }

        public bool Blocked
        {
            get { return _blocked(); }
        }

        public ISubscriberConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber)
        {
            _errorSubscriber = errorSubscriber;

            return this;
        }

        public ISubscriberConfigurator UseTrace(ITrace trace)
        {
            _trace = trace;

            return this;
        }

        public ISubscriberConfigurator UseDurableQueue(string queueName)
        {
            _durable = true;

            _queueName = queueName;

            _autoCreate = false;

            return this;
        }

        public ISubscriberConfigurator UseDurableQueue(string queueName, bool createBindings)
        {
            _durable = true;

            _queueName = queueName;

            _createBindings = createBindings;

            _autoCreate = false;

            return this;
        }

        public ISubscriberConfigurator UseDurableQueue(string queueName, bool createBindings, bool autoCreate)
        {
            _durable = true;

            _queueName = queueName;

            _createBindings = createBindings;

            _autoCreate = autoCreate;

            return this;
        }

        public ISubscriberConfigurator UseNonDurableNamedQueue(string queueName)
        {
            _isExclusive = true;

            _durable = false;

            _autoCreate = true;

            _queueName = queueName;

            return this;
        }
        public ISubscriberConfigurator UseNonDurableNamedQueue(string queueName, bool isExclusive)
        {
            _isExclusive = isExclusive;

            _durable = false;

            _autoCreate = true;

            _queueName = queueName;

            return this;
        }

        public ISubscriberConfigurator UseTransactionalDelivery()
        {
            _transactionalDelivery = true;

            return this;
        }

        public ISubscriberConfigurator UseTransactionalDelivery(ushort prefetch)
        {
            _transactionalDelivery = true;
            _prefetch = prefetch;

            return this;
        }

        public ISubscriberConfigurator UseTransactionalDelivery(IExceptionFilter exceptionFilter)
        {
            _transactionalDelivery = true;
            _exceptionFilter = exceptionFilter;

            return this;
        }

        public ISubscriberConfigurator UseTransactionalDelivery(IExceptionFilter exceptionFilter, ushort prefetch)
        {
            _transactionalDelivery = true;
            _exceptionFilter = exceptionFilter;
            _prefetch = prefetch;

            return this;
        }

        public ISubscriberConfigurator SetReceiveSelfPublish(bool receive)
        {
            _receiveSelfPublish = receive;

            return this;
        }

        public ISubscriberConfigurator SetConsumerTag(string consumerTag)
        {
            _consumerTag = consumerTag;

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

        public ISubscriberConfigurator UseJsonSerializerSettings(JsonSerializerSettings settings)
        {
            _settings = settings;

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

        public ISubscriberConfigurator SetMaxPriority(sbyte maxPriority)
        {
            _maxPriority = maxPriority;

            return this;
        }
    }
}