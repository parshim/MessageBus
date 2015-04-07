namespace MessageBus.Core
{
    public class SubscriptionInfo
    {
        public ICallHandler Handler { get; set; }

        public MessageFilterInfo FilterInfo { get; set; }
    }
}