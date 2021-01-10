using System;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Core.API;
using Newtonsoft.Json;

namespace MessageBus.Core
{
    public class PublisherConfigurator : IPublisherConfigurator
    {
        private IPublishingErrorHandler _errorHandler;
        private ITrace _trace;
        private readonly Func<bool> _blocked;
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
        
        public PublisherConfigurator(string exchange, IPublishingErrorHandler errorHandler, ITrace trace, Func<bool> blocked)
        {
            _exchange = exchange;
            _errorHandler = errorHandler;
            _trace = trace;
            _blocked = blocked;
        }

        public IPublishingErrorHandler ErrorHandler
        {
            get
            {
                return _errorHandler;
            }
        }

        public ITrace Trace
        {
            get
            {
                return _trace;
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

        public bool Blocked
        {
            get { return _blocked(); }
        }

        public IPublisherConfigurator UseErrorHandler(IPublishingErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;

            return this;
        }

        public IPublisherConfigurator UseTrace(ITrace trace)
        {
            _trace = trace;

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

        public IPublisherConfigurator UseXmlSerializer()
        {
            _serializer = new XmlSerializer();

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