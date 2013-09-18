using System.ServiceModel;

namespace RabbitMQ.IntegrationTests.ContractsAndServices
{
    [ServiceContract]
    public interface IOneWayService
    {
        [OperationContract(IsOneWay = true)]
        void Say(Data data);
    }
}