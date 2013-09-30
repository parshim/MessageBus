using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public abstract class Bus : IBus
    {
        private readonly System.ServiceModel.Channels.Binding _binding;

        private readonly Uri _output;
        private readonly Uri _input;

        private readonly IChannelFactory<IOutputChannel> _channelFactory;

        private readonly string _busId;

        protected Bus(System.ServiceModel.Channels.Binding binding, Uri output, Uri input, string busId)
        {
            _binding = binding;
            _output = output;
            _input = input;
            _busId = busId;

            _channelFactory = _binding.BuildChannelFactory<IOutputChannel>();

            _channelFactory.Open();

        }

        public void Dispose()
        {
            _channelFactory.Close();
        }

        public IPublisher CreatePublisher()
        {
            IOutputChannel outputChannel = _channelFactory.CreateChannel(new EndpointAddress(_output));

            return new Publisher(outputChannel, _binding.MessageVersion, _busId);
        }

        public ISubscriber CreateSubscriber()
        {
            IChannelListener<IInputChannel> listener = _binding.BuildChannelListener<IInputChannel>(_input);

            return new Subscriber(listener, _busId);
        }

    }
}
