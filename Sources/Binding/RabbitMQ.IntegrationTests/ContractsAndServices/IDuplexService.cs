using System.ServiceModel;

namespace RabbitMQ.IntegrationTests.ContractsAndServices
{
    [ServiceContract(CallbackContract = typeof(IDuplexService))]
    public interface IDuplexService
    {
        [OperationContract(IsOneWay = true)]
        void Say(Data data);
    }
}