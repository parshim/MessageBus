using System.Collections.Generic;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal abstract class SubscriptionBase : ISubscription
    {
        private readonly IMessageFilter _messageFilter;

        protected readonly IInputChannel _inputChannel;
        protected readonly IDispatcher _dispatcher;

        protected bool _stared;
        
        protected SubscriptionBase(IInputChannel inputChannel, IMessageFilter messageFilter, IDispatcher dispatcher)
        {
            _inputChannel = inputChannel;
            _messageFilter = messageFilter;
            _dispatcher = dispatcher;
        }

        public virtual void Open()
        {
            if (_stared) return;

            _inputChannel.Open();

            _stared = true;

            ApplyFilters();
        }

        public virtual void Close()
        {
            if (_stared)
            {
                _inputChannel.Close();
            }
        }

        protected void ApplyFilters()
        {
            IEnumerable<MessageFilterInfo> filters = _dispatcher.GetApplicableFilters();

            _messageFilter.ApplyFilters(filters);
        }

        public void Dispose()
        {
            Close();
        }
    }
}