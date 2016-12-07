using System;
using System.Configuration;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class BusConfigurator : IBusConfigurator
    {
        private string _busId = Guid.NewGuid().ToString();
        private IPublishingErrorHandler _errorHandler;
        private ITrace _trace;
        private IErrorSubscriber _errorSubscriber;
        private RabbitMQConnectionString _connectionString;
        private bool _receiveSelfPublish;
        private bool _useFastReply = true;
        private string _replyExchange = "";

        public string BusId
        {
            get { return _busId; }
        }

        public IPublishingErrorHandler ErrorHandler
        {
            get
            {
                return _errorHandler ?? new NullPublishingErrorHandler();
            }
        }
        
        public IErrorSubscriber ErrorSubscriber
        {
            get
            {
                return _errorSubscriber ?? new NullErrorSubscriber();
            }
        }

        public ITrace Trace
        {
            get
            {
                return _trace ?? new NullTrace();
            }
        }

        public bool ReceiveSelfPublish
        {
            get
            {
                return _receiveSelfPublish;
            }
        }

        public bool UseFastReply
        {
            get { return _useFastReply; }
        }

        public string ReplyExchange
        {
            get { return _replyExchange; }
        }

        public IBusConfigurator SetBusId(string busId)
        {
            _busId = busId;

            return this;
        }

        public RabbitMQConnectionString ConnectionString
        {
            get { return _connectionString ?? new RabbitMQConnectionString(); }
        }

        public IBusConfigurator UseConnectionString(string nameOrConnectionString)
        {
            var configConnectionString = ConfigurationManager.ConnectionStrings[nameOrConnectionString];
            var connectionString = configConnectionString?.ConnectionString ?? nameOrConnectionString;
            var connectionUri = new Uri(connectionString);

            _connectionString = new RabbitMQConnectionString(connectionUri);

            return this;
        }

        public IBusConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber)
        {
            _errorSubscriber = errorSubscriber;

            return this;
        }

        public IBusConfigurator UseErrorHandler(IPublishingErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;

            return this;
        }

        public IBusConfigurator UseTrace(ITrace trace)
        {
            _trace = trace;

            return this;
        }

        public IBusConfigurator SetReceiveSelfPublish()
        {
            _receiveSelfPublish = true;

            return this;
        }

        public IBusConfigurator DisableFastReply()
        {
            _useFastReply = false;

            return this;
        }

        public IBusConfigurator SetReplyExchange(string replyExchange)
        {
            _replyExchange = replyExchange;

            return this;
        }
    }
}