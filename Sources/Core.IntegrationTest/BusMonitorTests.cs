using System;
using System.Threading;
using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;
using FluentAssertions;

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

        [Test]
        public void Bus_PublishRawMessage_MonitorReceive()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            using (IBus bus = new RabbitMQBus())
            {
                RawBusMessage actual = null;
                RawBusMessage expected = new RawBusMessage
                {
                    Name = "MyName",
                    Namespace = "MyNamespace",
                    Data = "Some raw data"
                };

                Action<RawBusMessage> action = message =>
                {
                    actual = message;
                    ev.Set();
                };

                using (ISubscription monitor = bus.CreateMonitor(action))
                {
                    monitor.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(expected);
                    }

                    var waitOne = ev.WaitOne(TimeSpan.FromSeconds(10));

                    Assert.IsTrue(waitOne);

                    actual.Should().NotBeNull();
                    actual.Name.Should().Be(expected.Name);
                    actual.Namespace.Should().Be(expected.Namespace);
                    actual.Data.Should().Be(string.Format("\"{0}\"", expected.Data));
                }
            }
        }
    }
}