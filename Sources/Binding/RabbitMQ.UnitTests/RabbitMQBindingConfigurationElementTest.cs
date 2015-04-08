using MessageBus.Binding.RabbitMQ;
using FluentAssertions;
using NUnit.Framework;

namespace RabbitMQ.UnitTests
{
    [TestFixture]
    public class ConfigurationElementTest
    {
        [Test]
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
