using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusSubscribtionsTests
    {
        [TestMethod]
        public void Bus_PublishedMessage_ReceiveSelfPublishIsFalse_ShouldNotArriveToSubscriber()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            using (RabbitMQBus bus = new RabbitMQBus())
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((Action<OK>) (ok => ev.Set()), receiveSelfPublish: false);
                    
                    subscriber.StartProcessMessages();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new OK());
                    }

                    bool wait = ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should()
                        .BeFalse("Message should not arrive from publisher to subscriber within same bus instance");
                }
            }
        }
        
        [TestMethod]
        public void Bus_PublishedMessage_ReceiveSelfPublishIsTrue_ShouldArriveToSubscriber()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            using (var bus = new RabbitMQBus())
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((Action<OK>) (ok => ev.Set()), receiveSelfPublish: true);
                    
                    subscriber.StartProcessMessages();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new OK());
                    }

                    bool wait = ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should()
                        .BeTrue("Message should not arrive from publisher to subscriber within same bus instance");
                }
            }
        }

        [TestMethod]
        public void Bus_PublishedMessage_EachSubscriberGetsMessageCopy()
        {
            Person actual = new Person
                {
                    Id = 5
                };

            Person b1 = null, b2 = null, c1 = null;

            ManualResetEvent ev1 = new ManualResetEvent(false), ev2 = new ManualResetEvent(false), ev3 = new ManualResetEvent(false);

            using (RabbitMQBus busA = new RabbitMQBus(), busB = new RabbitMQBus(), busC = new RabbitMQBus())
            {
                using (ISubscriber subscriberB1 = busB.CreateSubscriber(), subscriberB2 = busB.CreateSubscriber(), subscriberC1 = busC.CreateSubscriber())
                {
                    subscriberB1.Subscribe<Person>(p => { b1 = p; ev1.Set(); });
                    subscriberB2.Subscribe<Person>(p => { p.Id *= 2; b2 = p; ev2.Set(); });
                    subscriberC1.Subscribe<Person>(p => { c1 = p; ev3.Set(); });

                    subscriberB1.StartProcessMessages();
                    subscriberB2.StartProcessMessages();
                    subscriberC1.StartProcessMessages();

                    using (IPublisher publisher = busA.CreatePublisher())
                    {
                        publisher.Send(actual);
                    }

                    bool wait = ev1.WaitOne(TimeSpan.FromSeconds(5)) &&
                                ev2.WaitOne(TimeSpan.FromSeconds(5)) &&
                                ev3.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue("Message should arrive to all subscribers");

                    b1.Should().NotBeNull();
                    b2.Should().NotBeNull();
                    c1.Should().NotBeNull();

                    b1.ShouldBeEquivalentTo(actual);
                    b2.ShouldBeEquivalentTo(new Person
                        {
                            Id = actual.Id * 2
                        });
                    c1.ShouldBeEquivalentTo(actual);

                }
            }
        }
    }
}