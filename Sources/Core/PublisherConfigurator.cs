using System;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class PublisherConfigurator : IPublisherConfigurator
    {
        private BufferManager _bufferManager;
        private IPublishingErrorHandler _errorHandler;

        internal BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        internal FaultMessageProcessor FaultMessageProcessor
        {
            get
            {
                return new FaultMessageProcessor(_errorHandler ?? new NullPublishingErrorHandler());
            }
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
    }

    internal class NullPublishingErrorHandler : IPublishingErrorHandler
    {
        public void DeliveryFailed(int errorCode, string text, RawBusMessage message)
        {
            
        }
    }
}