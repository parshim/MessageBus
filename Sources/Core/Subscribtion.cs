using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal sealed class Subscribtion : SubscriberBase
    {
        private readonly ISubscriptionDispatcher _dispatcher;
        private readonly object _instance;

        public Subscribtion(IInputChannel inputChannel, IMessageFilter messageFilter, ISubscriptionDispatcher dispatcher, object instance)
            : base(inputChannel, messageFilter, dispatcher)
        {
            _dispatcher = dispatcher;
            _instance = instance;
        }

        public override void Open()
        {
            _dispatcher.RegisterSubscribtion(_instance);

            base.Open();
        }
    }
}