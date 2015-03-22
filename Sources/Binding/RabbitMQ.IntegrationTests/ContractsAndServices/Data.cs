using System.Runtime.Serialization;

namespace RabbitMQ.IntegrationTests.ContractsAndServices
{
    [DataContract]
    [KnownType(typeof(ExtraData))]
    public class Data
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class ExtraData : Data
    {
        [DataMember]
        public int Age { get; set; }
    }

    [DataContract]
    public class Blob
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public byte[] Data { get; set; }
    }
}