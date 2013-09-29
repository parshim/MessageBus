using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public abstract class Bus : IBus
    {
        private readonly System.ServiceModel.Channels.Binding _binding;

        private readonly IChannelFactory<IOutputChannel> _channelFactory;

        protected Bus(System.ServiceModel.Channels.Binding binding)
        {
            _binding = binding;

            _channelFactory = _binding.BuildChannelFactory<IOutputChannel>();

            _channelFactory.Open();

        }

        public void Dispose()
        {
            _channelFactory.Close();
        }

        public IPublisher CreatePublisher()
        {
            IOutputChannel outputChannel = _channelFactory.CreateChannel(new EndpointAddress("amqp://localhost/amq.fanout"));

            return new Publisher(outputChannel, _binding.MessageVersion);
        }

        public ISubscriber CreateSubscriber()
        {
            IChannelListener<IInputChannel> listener = _binding.BuildChannelListener<IInputChannel>(new Uri("amqp://localhost/"));

            return new Subscriber(listener);
        }
    }
}
