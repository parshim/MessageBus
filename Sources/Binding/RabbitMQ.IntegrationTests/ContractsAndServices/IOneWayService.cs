using System.ServiceModel;

namespace RabbitMQ.IntegrationTests.ContractsAndServices
{
    [ServiceContract]
    public interface IOneWayService
    {
        [OperationContract(IsOneWay = true, Action = "Say")]
        void Say(Data data);

        [OperationContract(IsOneWay = true, Action = "LargeData")]
        void LargeData(Blob data);
    }
    [ServiceContract]
    public interface IOneWayService2
    {
        [OperationContract(IsOneWay = true)]
        void Say2(Data data);
    }
}