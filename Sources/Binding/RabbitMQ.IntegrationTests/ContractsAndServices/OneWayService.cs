using System.ServiceModel;

namespace RabbitMQ.IntegrationTests.ContractsAndServices
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class OneWayService : IOneWayService, IOneWayService2
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

        public void LargeData(Blob data)
        {
            _errorProcessor.LargeData(data);
            _processor.LargeData(data);
        }

        public void Say2(Data data)
        {
            _errorProcessor.Say(data);
        }
    }
}
