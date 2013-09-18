using System.ServiceModel;

namespace RabbitMQ.IntegrationTests.ContractsAndServices
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class DuplexService : IDuplexService
    {
        private readonly IDuplexService _process;
        private readonly Data _replyData;

        public DuplexService(IDuplexService process, Data replyData)
        {
            _process = process;
            _replyData = replyData;
        }

        public void Say(Data data)
        {
            _process.Say(data);

            IDuplexService callbackChannel = OperationContext.Current.GetCallbackChannel<IDuplexService>();
            
            callbackChannel.Say(_replyData);
        }
    }
}
