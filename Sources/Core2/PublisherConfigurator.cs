using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using MessageBus.Core.API;
using Newtonsoft.Json;

namespace MessageBus.Core
{
    public class PublisherConfigurator : IPublisherConfigurator
    {
        private BufferManager _bufferManager;
        private IPublishingErrorHandler _errorHandler;
        private ISerializer _serializer;
        private bool _mandatoryDelivery;
        private bool _persistentDelivery;
        private JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None
        };

        private string _exchange;
        private string _routingKey = "";
        private string _replyTo;
        private IEnumerable<BusHeader> _headers = Enumerable.Empty<BusHeader>();
        
        public PublisherConfigurator(string exchange, IPublishingErrorHandler errorHandler)
        {
            _exchange = exchange;
            _errorHandler = errorHandler;
        }

        public BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        public IPublishingErrorHandler ErrorHandler
        {
            get
            {
                return _errorHandler;
            }
        }

        public bool MandatoryDelivery
        {
            get { return _mandatoryDelivery; }
        }

        public bool PersistentDelivery
        {
            get { return _persistentDelivery; }
        }

        public ISerializer Serializer
        {
            get { return _serializer ?? new JsonSerializer(_settings); }
        }

        public string Exchange
        {
            get { return _exchange; }
        }

        public string RoutingKey
        {
            get { return _routingKey; }
        }

        public IEnumerable<BusHeader> Headers
        {
            get { return _headers; }
        }

        public string ReplyTo
        {
            get { return _replyTo; }
        }

        public IPublisherConfigurator UseBufferManager(BufferManager bufferManager)
        {
            _bufferManager = bufferManager;

            return this;
        }

        public IPublisherConfigurator UseErrorHandler(IPublishingErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;

            return this;
        }

        public IPublisherConfigurator SetMandatoryDelivery()
        {
            _mandatoryDelivery = true;

            return this;
        }

        public IPublisherConfigurator SetPersistentDelivery()
        {
            _persistentDelivery = true;

            return this;
        }

        public IPublisherConfigurator UseSoapSerializer()
        {
            _serializer = new SoapSerializer();
            
            return this;
        }

        public IPublisherConfigurator UseCustomSerializer(ISerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public IPublisherConfigurator UseJsonSerializerSettings(JsonSerializerSettings settings)
        {
            _settings = settings;

            return this;
        }

        public IPublisherConfigurator SetExchange(string exchange)
        {
            _exchange = exchange;

            return this;
        }

        public IPublisherConfigurator SetRoutingKey(string routingKey)
        {
            _routingKey = routingKey;

            return this;
        }
        public IPublisherConfigurator SetReplyTo(string replyTo)
        {
            _replyTo = replyTo;

            return this;
        }

        public IPublisherConfigurator SetDefaultHeaders(IEnumerable<BusHeader> headers)
        {
            _headers = headers;

            return this;
        }
    }
}