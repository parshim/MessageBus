using MessageBus.Binding.RabbitMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace RabbitMQ.UnitTests
{
    [TestClass]
    public class ConfigurationElementTest
    {
        [TestMethod]
        public void RabbitMQBinding_ConfigurationElement_CreateInstance()
        {
            RabbitMQBindingConfigurationElement element = new RabbitMQBindingConfigurationElement("Binding0");
            
            element.ExactlyOnce.Should().BeFalse();
            element.PersistentDelivery.Should().BeFalse();
            element.OneWayOnly.Should().BeTrue();
            element.TTL.Should().BeNull();
            element.ReplyToExchange.Should().BeNull();
            element.ReplyToQueue.Should().BeNull();
            element.AutoBindExchange.Should().BeNull();
            element.ApplicationId.Should().BeNull();
            element.HeaderNamespace.Should().BeNull();
            element.MessageFormat.Should().Be(MessageFormat.Text);
            element.ProtocolVersion.Should().Be("DefaultProtocol");
            element.ReaderQuotas.Should().NotBeNull();
            element.Mandatory.Should().BeFalse();
            element.Immediate.Should().BeFalse();
        }

    }

}
