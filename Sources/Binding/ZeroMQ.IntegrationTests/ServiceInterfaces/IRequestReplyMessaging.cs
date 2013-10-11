using System.Runtime.Serialization;
using System.ServiceModel;

namespace ZeroMQ.IntegrationTests.ServiceInterfaces
{
    [ServiceContract]
    public interface IRequestReplyMessaging
    {
        [OperationContract]
        Result Operation(Request data);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class RequestReplyMessaging : IRequestReplyMessaging
    {
        private readonly IRequestReplyMessaging _wrapped;

        public RequestReplyMessaging(IRequestReplyMessaging wrapped)
        {
            _wrapped = wrapped;
        }

        public Result Operation(Request data)
        {
            return _wrapped.Operation(data);
        }
    }

    [DataContract]
    public class Request
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class Result
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}