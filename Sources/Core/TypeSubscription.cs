using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal sealed class TypeSubscription : MessagePumpSubscriptionBase
    {
        private readonly ISubscriptionDispatcher _dispatcher;
        private readonly object _instance;

        public TypeSubscription(IInputChannel inputChannel, IMessageFilter messageFilter, ISubscriptionDispatcher dispatcher, object instance)
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