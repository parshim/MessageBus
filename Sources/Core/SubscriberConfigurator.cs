using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class SubscriberConfigurator : ISubscriberConfigurator
    {
        private BufferManager _bufferManager;
        private IErrorSubscriber _errorSubscriber;
        private string _queueName;

        internal string QueueName
        {
            get { return _queueName; }
        }

        internal BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        internal IErrorSubscriber ErrorSubscriber
        {
            get { return _errorSubscriber ?? new NullErrorSubscriber(); }
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
    }
}