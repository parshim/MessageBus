using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public interface ISendHelper
    {
        void Send(SendParams sendParams);
        
        Type GetDataType(DataContractKey dataContractKey);
    }

    public class SendParams
    {
        public IModel Model { get; set; }

        public RawBusMessage BusMessage { get; set; }

        public string RoutingKey { get; set; }

        public string Exchange { get; set; }

        public string CorrelationId { get; set; }

        public ISerializer Serializer { get; set; }

        public string BusId { get; set; }

        public bool MandatoryDelivery { get; set; }
        
        public bool PersistentDelivery { get; set; }
        
        public string ReplyTo { get; set; }

        public byte? Priority { get; set; }
    }
}