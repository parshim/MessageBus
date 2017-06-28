using System;
using System.Collections.Generic;
using System.Text;
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
                    Namespace = "",
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
                    actual.Name.Should().Be("MyName");
                    actual.Namespace.Should().Be("");

                    Assert.AreEqual(actual.Data, Encoding.UTF8.GetBytes($"\"{expected.Data}\""));
                }
            }
        }

        [Test]
        public void Bus_PublishRawMessages_MonitorReceiveNotFiltered()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            using (IBus bus = new RabbitMQBus())
            {
                List<RawBusMessage> actual = new List<RawBusMessage>();

                RawBusMessage toFilterOut = new RawBusMessage
                {
                    Name = "SomeOtherName",
                    Namespace = "",
                    Data = "Some raw data"
                };

                RawBusMessage expected = new RawBusMessage
                {
                    Name = "MyName",
                    Namespace = "",
                    Data = "Some raw data"
                };

                Action<RawBusMessage> action = message =>
                {
                    actual.Add(message);
                };

                using (ISubscription monitor = bus.CreateMonitor(action, filterHeaders: new [] { new BusHeader("Name", "MyName") }))
                {
                    monitor.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(toFilterOut);
                        publisher.Send(expected);
                    }

                    ev.WaitOne(TimeSpan.FromSeconds(10));
                    
                    actual.Count.Should().Be(1);
                    actual[0].Name.Should().Be("MyName");
                    actual[0].Namespace.Should().Be("");

                    Assert.AreEqual(actual[0].Data, Encoding.UTF8.GetBytes($"\"{expected.Data}\""));
                }
            }
        }
    }
}