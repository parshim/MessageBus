using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Threading;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal abstract class SubscriberBase : IDisposable
    {
        private readonly IDispatcher _dispatcher;
        private readonly IInputChannel _inputChannel;
        private readonly Thread _receiver;
        private bool _receive;
        private bool _stared;
        
        private readonly IMessageFilter _messageFilter;

        protected SubscriberBase(IInputChannel inputChannel, IMessageFilter messageFilter, IDispatcher dispatcher)
        {
            _messageFilter = messageFilter;
            _dispatcher = dispatcher;
            _inputChannel = inputChannel;
            _receiver = new Thread(MessagePump);
        }

        private void MessagePump()
        {
            while (_receive)
            {
                Message message;

                if (!_inputChannel.TryReceive(TimeSpan.FromMilliseconds(100), out message))
                {
                    continue;
                }

                using (message)
                {
                    _dispatcher.Dispatch(message);
                }
            }
        }

        public void StartProcessMessages()
        {
            if (_stared) return;

            _inputChannel.Open();

            _receive = true;
            _stared = true;

            ApplyFilters();

            _receiver.Start();
        }

        protected void ApplyFilters()
        {
            IEnumerable<MessageFilterInfo> filters = _dispatcher.GetApplicableFilters();
            
            _messageFilter.ApplyFilters(filters);
        }

        public void Dispose()
        {
            _receive = false;

            if (_stared)
            {
                _receiver.Join(TimeSpan.FromMilliseconds(200));

                _inputChannel.Close();
            }
        }
    }

}