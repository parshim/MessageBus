using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public abstract class Bus : IBus
    {
        private readonly string _busId;
        private readonly IErrorSubscriber _errorSubscriber;

        protected Bus(string busId, IErrorSubscriber errorSubscriber)
        {
            _busId = busId;

            _errorSubscriber = errorSubscriber ?? new NullErrorSubscriber();
        }
        
        public IPublisher CreatePublisher()
        {
            IOutputChannel outputChannel = CreateOutputChannel();
            
            return new Publisher(outputChannel, MessageVersion, _busId);
        }

        public ISubscriber CreateSubscriber()
        {
            IInputChannel inputChannel = CreateInputChannel();

            if (inputChannel == null)
            {
                throw new NoIncomingConnectionAcceptedException();
            }

            return new Subscriber(inputChannel, _busId, _errorSubscriber);
        }

        protected abstract MessageVersion MessageVersion { get; }

        protected abstract IOutputChannel CreateOutputChannel();

        protected abstract IInputChannel CreateInputChannel();
        
        public abstract void Dispose();
    }
}
