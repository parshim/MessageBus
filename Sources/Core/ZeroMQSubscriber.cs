using System.Collections.Generic;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class ZeroMQSubscriber : Subscriber
    {
        public ZeroMQSubscriber(IInputChannel inputChannel, string busId, IErrorSubscriber errorSubscriber) : base(inputChannel, busId, errorSubscriber)
        {

        }

        protected override void ApplyFilters(IEnumerable<MessageFilterInfo> filters)
        {
            // TODO: Use ZMQ Pub\Sub filtering mechanism
        }
    }
}