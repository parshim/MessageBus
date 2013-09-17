using System.Runtime.Serialization;
using System.ServiceModel;

namespace RabbitMQ.IntegrationTests.Interfaces
{
    [ServiceContract]
    public interface IOneWayService
    {
        [OperationContract(IsOneWay = true)]
        void Say(Data data);
    }

    [DataContract]
    public class Data
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class OneWayService : IOneWayService
    {
        private readonly IOneWayService _impl;

        public OneWayService(IOneWayService impl)
        {
            _impl = impl;
        }

        public void Say(Data data)
        {
            _impl.Say(data);
        }
    }
}
