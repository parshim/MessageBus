using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
    public interface IFaultMessageProcessor
    {
        void Process(int code, string text, Message message);
    }
}