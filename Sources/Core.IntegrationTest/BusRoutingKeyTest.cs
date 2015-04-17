using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusRoutingKeyTest
    {
        [Test]
        public void SendAndReciveMessageFromFanoutExchangeWithRoutingKey()
        {
            using (IBus busA = new RabbitMQBus(), busB = new RabbitMQBus())
            {
                Person expected = new Person
                {
                    Id = 5
                };

                Person actual = null;

                ManualResetEvent ev = new ManualResetEvent(false);

                using (var subscriber = busB.CreateSubscriber(c => c.SetExchange("amq.fanout").SetRoutingKey("MyKey")))
                {
                    subscriber.Subscribe((Person p) =>
                    {
                        actual = p;

                        ev.Set();
                    });

                    subscriber.Open();

                    using (var publisher = busA.CreatePublisher(c => c.SetExchange("amq.fanout").SetRoutingKey("MyKey")))
                    {
                        publisher.Send(expected);
                    }

                    bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(20));

                    waitOne.Should().BeTrue("Message not received");

                    actual.ShouldBeEquivalentTo(expected);
                }
            }
        }
    }
}
