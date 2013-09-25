using MessageBus.Binding.RabbitMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace RabbitMQ.UnitTests
{
    [TestClass]
    public class RabbitMQBindingConfigurationElementTest
    {
        [TestMethod]
        public void RabbitMQBindingConfigurationElement_CreateInstance()
        {
            RabbitMQBindingConfigurationElement element = new RabbitMQBindingConfigurationElement("Binding0");
            
            element.IgnoreSelfPublished.Should().BeTrue();
            element.ExactlyOnce.Should().BeFalse();
            element.PersistentDelivery.Should().BeFalse();
            element.OneWayOnly.Should().BeTrue();
            element.TTL.Should().BeBlank();
            element.ReplyToExchange.Should().BeBlank();
            element.ReplyToQueue.Should().BeBlank();
            element.AutoBindExchange.Should().BeBlank();
            element.ApplicationId.Should().BeBlank();
            element.ProtocolVersion.Should().Be("DefaultProtocol");
        }

    }

}
