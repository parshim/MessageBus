using System.Configuration;
using FluentAssertions;
using MessageBus.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.UnitTests
{
    [TestClass]
    public class RabbitMQBusConfigSectionHandlerTest
    {
        [TestMethod]
        public void TestConfiguration()
        {
            RabbitMQBusConfigSectionHandler section = (RabbitMQBusConfigSectionHandler) ConfigurationManager.GetSection(RabbitMQBusConfigSectionHandler.SectionName);

            section.BrokerHost.Should().Be("myHost");
            section.Exchange.Should().Be("myExchange");
            section.Port.Should().Be(1234);
        }
    }
}
