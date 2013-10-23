using System;
using System.Linq;
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

        public IPublisher CreatePublisher(Action<IPublisherConfigurator> configure = null)
        {
            PublisherConfigurator configurator = new PublisherConfigurator();

            if (configure != null)
            {
                configure(configurator);
            }

            return OnCreatePublisher(configurator);
        }

        public ISubscriber CreateSubscriber(Action<ISubscriberConfigurator> configure = null)
        {
            SubscriberConfigurator configurator = new SubscriberConfigurator();

            if (configure != null)
            {
                configure(configurator);
            }

            return OnCreateSubscriber(configurator);
        }

        internal abstract IPublisher OnCreatePublisher(PublisherConfigurator configuration);

        internal abstract ISubscriber OnCreateSubscriber(SubscriberConfigurator configuration);

        protected object[] CreateParameters(params object[] parameters)
        {
            return parameters.Where(o => o != null).ToArray();
        }

        public abstract void Dispose();
    }
}
