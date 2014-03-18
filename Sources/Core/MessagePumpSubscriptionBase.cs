using System;
using System.ServiceModel.Channels;
using System.Threading;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal abstract class MessagePumpSubscriptionBase : SubscriptionBase
    {
        private readonly Thread _receiver;
        private bool _receive;
        
        protected MessagePumpSubscriptionBase(IInputChannel inputChannel, IMessageFilter messageFilter, IDispatcher dispatcher) : base(inputChannel, messageFilter, dispatcher)
        {
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

        public override void Open()
        {
            if (_stared) return;

            base.Open();
            
            _receive = true;
            
            _receiver.Start();
        }

        public override void Close()
        {
            _receive = false;

            if (_stared)
            {
                _receiver.Join(TimeSpan.FromMilliseconds(200));
            }

            base.Close();
        }

    }

}