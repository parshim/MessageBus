using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusMessageTests
    {
        [Test, Ignore("Fails on mono - need investigation")]
        public void Bus_BusMessage_HeadersAndPropertiesUsage()
        {
            const string busId = "Bus";

            using (MessageBus.Core.RabbitMQBus bus = new MessageBus.Core.RabbitMQBus(busId))
            {
                BusMessage<Person> message = new BusMessage<Person>
                    {
                        Data = new Person { Id = 5 }
                    };
                message.Headers.Add(new BusHeader
                    {
                       Name = "Version", Value = "Ver1"
                    });

                BusMessage<Person> received = null;
                
                ManualResetEvent ev = new ManualResetEvent(false);

                DateTime sent;
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.SetReceiveSelfPublish()))
                {
                    subscriber.Subscribe((Action<BusMessage<Person>>) (m =>
                        {
                            received = m;
                            ev.Set();
                        }));

                    subscriber.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        sent = DateTime.Now;
                        publisher.Send(message);
                    }

                    bool wait = ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue();
                }

                received.ShouldBeEquivalentTo(message, options => options.Excluding(m => m.BusId).Excluding(m => m.Sent));

                received.BusId.Should().Be(busId);
                received.Sent.Should().BeCloseTo(sent, 1000);
            }
        }
    }
}