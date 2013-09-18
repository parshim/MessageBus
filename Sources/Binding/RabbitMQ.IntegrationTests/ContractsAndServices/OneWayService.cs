using System.ServiceModel;

namespace RabbitMQ.IntegrationTests.ContractsAndServices
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class OneWayService : IOneWayService
    {
        private readonly IOneWayService _processor;
        private readonly IOneWayService _errorProcessor;

        public OneWayService(IOneWayService processor, IOneWayService errorProcessor)
        {
            _processor = processor;
            _errorProcessor = errorProcessor;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void Say(Data data)
        {
            _errorProcessor.Say(data);
            _processor.Say(data);
        }
    }
}
