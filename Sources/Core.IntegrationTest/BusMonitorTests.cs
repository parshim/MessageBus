using System;
using System.Threading;
using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusMonitorTests
    {
        [Test]
        public void Bus_PublishMessage_MonitorReceive()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            using (IBus bus = new RabbitMQBus())
            {
                Action<RawBusMessage> action = message =>
                {
                    ev.Set();
                };

                using (ISubscription monitor = bus.CreateMonitor(action))
                {
                    monitor.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new Person
                        {
                            Id = 34
                        });
                    }

                    var waitOne = ev.WaitOne(TimeSpan.FromSeconds(10));

                    Assert.IsTrue(waitOne);
                }
            }
        }
    }
}