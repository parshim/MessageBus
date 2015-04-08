using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using FluentAssertions;
using MessageBus.Binding.RabbitMQ;
using NUnit.Framework;

namespace RabbitMQ.IntegrationTests
{
    [TestFixture]
    public class MandatoryTest : IFaultMessageProcessor
    {
        private IChannelFactory<IOutputChannel> _channelFactory;
        private RabbitMQTransportOutputChannel _outputChannel;
        private RabbitMQBinding _binding;

        private MessageBuffer _buffer;
        private ManualResetEvent _ev;
        private int _code;
        private string _text;

        [TestFixtureSetUp]
        public void TestInitialize()
        {
            _ev = new ManualResetEvent(false);

            const string clientAddress = "amqp://localhost/amq.direct?routingKey=NoSuchRoute";

            _binding = new RabbitMQBinding
                {
                    OneWayOnly = true,
                    ApplicationId = "MyApp",
                    Mandatory = true
                };
            
            _channelFactory = _binding.BuildChannelFactory<IOutputChannel>(this);

            _channelFactory.Open();

            _outputChannel = _channelFactory.CreateChannel(new EndpointAddress(clientAddress)) as RabbitMQTransportOutputChannel;

            _outputChannel.Open();
        }

        [TestFixtureTearDown]
        public virtual void TestCleanup()
        {
            _channelFactory.Close(TimeSpan.FromSeconds(2));
            _outputChannel.Close(TimeSpan.FromSeconds(2));
        }

        [Test]
        public void RabbitMQBinding_ManadatoryDelivery_UnroutedMessageShouldReturn()
        {
            const string action = "Action";
            const string body = "Body";

            using (Message message = Message.CreateMessage(_binding.MessageVersion, action, body))
            {
                _outputChannel.Send(message);
            }

            bool res = _ev.WaitOne(TimeSpan.FromSeconds(10));

            res.Should().BeTrue();

            using (Message returned = _buffer.CreateMessage())
            {
                returned.Headers.Action.Should().Be(action);

                returned.GetBody<string>().Should().Be(body);
            }

            _code.Should().Be(312);
            _text.Should().Be("NO_ROUTE");
        }

        public void Process(int code, string text, Message message)
        {
            _code = code;
            _text = text;
            _buffer = message.CreateBufferedCopy(Int32.MaxValue);
            _ev.Set();
        }
    }
}
