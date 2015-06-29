using System;
using MessageBus.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.UnitTests
{
    [TestClass]
    public class RabbitMQConnectionStringTest
    {
        [TestMethod]
        public void FullUrl_AllValuesExtracted()
        {
            var cs = new RabbitMQConnectionString(new Uri("amqp://u:p@2.2.2.2:2020/vHost/exch?routingKey=key"));

            Assert.AreEqual("2.2.2.2", cs.Host);
            Assert.AreEqual(2020, cs.Port);
            Assert.AreEqual("vHost", cs.VirtualHost);
            Assert.AreEqual("exch", cs.Endpoint);
            Assert.AreEqual("u", cs.Username);
            Assert.AreEqual("p", cs.Password);
            Assert.AreEqual("key", cs.RoutingKey);
        }
        
        [TestMethod]
        public void OnlyHostAndPort_AllValuesExtracted()
        {
            var cs = new RabbitMQConnectionString(new Uri("amqp://2.2.2.2:2020"));

            Assert.AreEqual("2.2.2.2", cs.Host);
            Assert.AreEqual(2020, cs.Port);
            Assert.AreEqual("/", cs.VirtualHost);
            Assert.AreEqual("", cs.Endpoint);
        }

        [TestMethod]
        public void OnlyHost_AllValuesExtracted()
        {
            var cs = new RabbitMQConnectionString(new Uri("amqp://2.2.2.2"));

            Assert.AreEqual("2.2.2.2", cs.Host);
            Assert.AreEqual(-1, cs.Port);
            Assert.AreEqual("/", cs.VirtualHost);
            Assert.AreEqual("", cs.Endpoint);
        }
    }
}
