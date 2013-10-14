using System;
using System.Linq;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public abstract class Bus : IBus
    {
        private readonly string _busId;

        protected Bus(string busId)
        {
            _busId = busId ?? Guid.NewGuid().ToString();
        }

        public string BusId
        {
            get { return _busId; }
        }

        protected object[] CreateParameters(params object[] parameters)
        {
            return parameters.Where(o => o != null).ToArray();
        }

        public abstract IPublisher CreatePublisher(BufferManager bufferManager = null);

        public abstract ISubscriber CreateSubscriber(BufferManager bufferManager = null);
        
        public abstract void Dispose();
    }
}
