using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.UnitTests
{
    [TestFixture]
    public class RabbitMqBusConfigurationTests
    {
        [Test]
        public void RabbitMQBus_SetConnectionName_RabbitMQBusConnectionNameWasSet()
        {
            var expectedName = "test name";
            using (IBus bus = new RabbitMQBus(c => c.SetConnectionProvidedName(expectedName)))
            {
                Assert.AreEqual(expectedName, bus.BusConnectionName);
            }
        }
    }
}
