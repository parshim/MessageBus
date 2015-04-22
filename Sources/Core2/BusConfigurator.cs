using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class BusConfigurator : IBusConfigurator
    {
        private string _busId = Guid.NewGuid().ToString();
        private IPublishingErrorHandler _errorHandler;
        private IErrorSubscriber _errorSubscriber;
        private RabbitMQConnectionString _connectionString;

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

        public IBusConfigurator SetBusId(string busId)
        {
            _busId = busId;

            return this;
        }

        public RabbitMQConnectionString ConnectionString
        {
            get { return _connectionString ?? new RabbitMQConnectionString(); }
        }

        public IBusConfigurator UseConnectionString(string connectionString)
        {
            _connectionString = new RabbitMQConnectionString(new Uri(connectionString));

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
    }
}