using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusPerformanceTests
    {
        [TestMethod]
        public void Bus_SendReceive_1000Messages()
        {
            using (IBus entityA = new Bus(), entityB = new Bus())
            {
                Data messageA = new Person
                    {
                        Id = 5
                    };

                ManualResetEvent ev1 = new ManualResetEvent(false);

                int counter = 0;

                using (ISubscriber subscriberA = entityA.CreateSubscriber())
                {
                    subscriberA.SubscribeHierarchy<Data>(d => counter++);

                    subscriberA.Subscribe(typeof(OK), data => ev1.Set());

                    using (IPublisher publisher = entityB.CreatePublisher())
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            publisher.Send(messageA);
                        }

                        publisher.Send(new OK());
                    }

                    bool waitOne = ev1.WaitOne(TimeSpan.FromSeconds(20));

                    waitOne.Should().BeTrue("Message not received");

                    counter.Should().Be(1000);
                }
            }
        }
    }
}