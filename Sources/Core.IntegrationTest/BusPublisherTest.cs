using System;
using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusPublisherTest
    {
        [Test]
        public void WaitForConfirm_ConfirmShouldArrive()
        {
            using (IBus bus = new RabbitMQBus())
            {
                using (IConfirmPublisher publisher = bus.CreateConfirmPublisher())
                {
                    publisher.Send(new Person
                    {
                        Id = 5
                    });

                    var waitForConfirms = publisher.WaitForConfirms(TimeSpan.FromSeconds(10));

                    Assert.IsTrue(waitForConfirms);
                }
            }
        }
    }
}
