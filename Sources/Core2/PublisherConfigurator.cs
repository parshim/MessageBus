using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class PublisherConfigurator : IPublisherConfigurator
    {
        private BufferManager _bufferManager;
        private IPublishingErrorHandler _errorHandler;
        private ISerializer _serializer;
        private bool _mandatoryDelivery;

        public BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        public IPublishingErrorHandler ErrorHandler
        {
            get
            {
                return _errorHandler ?? new NullPublishingErrorHandler();
            }
        }

        public bool MandatoryDelivery
        {
            get { return _mandatoryDelivery; }
        }

        public ISerializer Serializer
        {
            get { return _serializer ?? new JsonSerializer(); }
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
    }
}