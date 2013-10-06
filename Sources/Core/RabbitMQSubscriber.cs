using System.Collections;
using System.Collections.Generic;
using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    internal class RabbitMQSubscriber : Subscriber
    {
        private readonly RabbitMQTransportInputChannel _inputChannel;
        private readonly string _exchange;

        public RabbitMQSubscriber(RabbitMQTransportInputChannel inputChannel, string exchange, string busId, IErrorSubscriber errorSubscriber)
            : base(inputChannel, busId, errorSubscriber)
        {
            _exchange = exchange;
            _inputChannel = inputChannel;
        }

        protected override void ApplyFilters(IEnumerable<MessageFilterInfo> filters)
        {
            IModel model = _inputChannel.Model;
            string queueName = _inputChannel.QueueName;

            foreach (MessageFilterInfo filter in filters)
            {
                IDictionary arguments = new Hashtable();

                arguments.Add(MessagingConstancts.HeaderNames.Name, filter.ContractKey.Name);
                arguments.Add(MessagingConstancts.HeaderNames.NameSpace, filter.ContractKey.Ns);

                foreach (BusHeader busHeader in filter.FilterHeaders)
                {
                    arguments.Add(busHeader.Name, busHeader.Value);
                }

                model.QueueBind(queueName, _exchange, "", arguments);
            }
        }
    }
}