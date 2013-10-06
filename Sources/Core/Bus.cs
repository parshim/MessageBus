using System;
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

        public abstract IPublisher CreatePublisher();

        public abstract ISubscriber CreateSubscriber();
    }
}
