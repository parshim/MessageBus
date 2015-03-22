using System.Collections;
using System.Collections.Generic;
using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    internal class RabbitMQMessageFilter : IMessageFilter
    {
        private readonly RabbitMQTransportInputChannel _inputChannel;
        private readonly string _exchange;

        public RabbitMQMessageFilter(RabbitMQTransportInputChannel inputChannel, string exchange)
        {
            _inputChannel = inputChannel;
            _exchange = exchange;
        }

        public void ApplyFilters(IEnumerable<MessageFilterInfo> filters)
        {
            IModel model = _inputChannel.Model;
            string queueName = _inputChannel.QueueName;

            foreach (MessageFilterInfo filter in filters)
            {
                IDictionary<string, object> arguments = new Dictionary<string, object>();

                arguments.Add(MessagingConstants.HeaderNames.Name, filter.ContractKey.Name);
                arguments.Add(MessagingConstants.HeaderNames.NameSpace, filter.ContractKey.Ns);

                foreach (BusHeader busHeader in filter.FilterHeaders)
                {
                    arguments.Add(busHeader.Name, busHeader.Value);
                }

                model.QueueBind(queueName, _exchange, "", arguments);
            }
        }
    }
}