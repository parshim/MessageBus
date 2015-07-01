using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using MessageBus.Core.API;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace MessageBus.Core
{
    public class Publisher : IPublisher
    {
        private readonly ConcurrentDictionary<Type, DataContractKey> _nameMappings = new ConcurrentDictionary<Type, DataContractKey>();

        protected readonly IModel _model;

        private readonly IMessageHelper _messageHelper;

        private readonly string _busId;

        private readonly PublisherConfigurator _configuration;

        public Publisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper)
        {
            _model = model;
            _configuration = configuration;
            _messageHelper = messageHelper;
            _busId = busId;

            _model.BasicReturn += ModelOnBasicReturn;
        }

        private void ModelOnBasicReturn(object sender, BasicReturnEventArgs args)
        {
            DataContractKey dataContractKey = args.BasicProperties.GetDataContractKey();

            Type dataType = _nameMappings.Where(pair => pair.Value.Equals(dataContractKey)).Select(pair => pair.Key).FirstOrDefault();

            if (dataType == null)
            {
                dataContractKey = DataContractKey.BinaryBlob;
            }

            object data = _configuration.Serializer.Deserialize(dataContractKey, dataType, args.Body);

            RawBusMessage message = _messageHelper.ConstructMessage(dataContractKey, args.BasicProperties, data);

            _configuration.ErrorHandler.DeliveryFailed(args.ReplyCode, args.ReplyText, message);
        }

        public void Dispose()
        {
            _model.BasicReturn -= ModelOnBasicReturn;

            _model.Close();
        }

        public void Send<TData>(TData data)
        {
            Send(new RawBusMessage { Data = data });
        }

        public void Send<TData>(BusMessage<TData> busMessage)
        {
            RawBusMessage rawBusMessage = new RawBusMessage
            {
                Data = busMessage.Data
            };

            foreach (BusHeader header in busMessage.Headers)
            {
                rawBusMessage.Headers.Add(header);
            }

            Send(rawBusMessage);
        }

        public void Send(RawBusMessage busMessage)
        {
            DataContractKey contractKey;
            Type type = busMessage.Data.GetType();

            if (!_nameMappings.TryGetValue(type, out contractKey))
            {
                contractKey = type.GetDataContractKey();

                _nameMappings.TryAdd(type, contractKey);
            }

            busMessage.Sent = DateTime.Now;
            busMessage.BusId = _busId;

            BasicProperties basicProperties = new BasicProperties
            {
                AppId = busMessage.BusId,
                Timestamp = busMessage.Sent.ToAmqpTimestamp(),
                Type = contractKey.Name,
                ContentType = _configuration.Serializer.ContentType,
                Headers = new Dictionary<string, object>
                {
                    {MessagingConstants.HeaderNames.Name, contractKey.Name},
                    {MessagingConstants.HeaderNames.NameSpace, contractKey.Ns}
                }
            };

            foreach (BusHeader header in busMessage.Headers)
            {
                basicProperties.Headers.Add(header.Name, header.Value);
            }

            if (_configuration.PersistentDelivery)
            {
                basicProperties.SetPersistent(true);
            }
            
            byte[] bytes = _configuration.Serializer.Serialize(busMessage);

            _model.BasicPublish(_configuration.Exchange, _configuration.RoutingKey, _configuration.MandatoryDelivery, false, basicProperties, bytes);
        }

    }
}