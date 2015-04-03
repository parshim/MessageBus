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
        public void Bus_SendReceive_BunchOfMessages()
        {
            using (RabbitMQBus entityA = new RabbitMQBus(), entityB = new RabbitMQBus())
            {
                Data messageA = new Person
                    {
                        Id = 5
                    };

                ManualResetEvent ev1 = new ManualResetEvent(false);

                int counter = 0;

                using (ISubscriber subscriberA = entityA.CreateSubscriber())
                {
                    subscriberA.Subscribe((Action<Data>)(d => counter++), true);

                    subscriberA.Subscribe(typeof(OK), (Action<object>)(data => ev1.Set()));

                    subscriberA.Open();

                    const int expected = 10;

                    using (IPublisher publisher = entityB.CreatePublisher())
                    {
                        for (int i = 0; i < expected; i++)
                        {
                            publisher.Send(messageA);
                        }

                        publisher.Send(new OK());
                    }

                    bool waitOne = ev1.WaitOne(TimeSpan.FromSeconds(20));

                    waitOne.Should().BeTrue("Message not received");

                    counter.Should().Be(expected);
                }
            }
        }
    }
}