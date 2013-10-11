using System.Runtime.Serialization;
using System.ServiceModel;

namespace ZeroMQ.IntegrationTests.ServiceInterfaces
{
    [ServiceContract]
    public interface IPublicationService
    {
        [OperationContract(IsOneWay = true)]
        void Notify(Data data);
    }


    [DataContract]
    public class Data
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PublicationService : IPublicationService
    {
        private readonly IPublicationService _wrapped;

        public PublicationService(IPublicationService wrapped)
        {
            _wrapped = wrapped;
        }

        public void Notify(Data data)
        {
            _wrapped.Notify(data);
        }
    }
}
