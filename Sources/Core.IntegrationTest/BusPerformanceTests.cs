using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusPerformanceTests
    {
        [Test]
        public void Bus_SendReceive_BunchOfMessages()
        {
            //RabbitMQConnectionString connectionString = new RabbitMQConnectionString(new Uri("amqp://rabbit:rabbit@10.0.8.100"));

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
                    subscriberA.Subscribe((Action<Data>)(d => Counter(ref counter)), true);

                    subscriberA.Subscribe(typeof(OK), (Action<object>)(data => ev1.Set()));

                    subscriberA.Open();

                    const int expected = 1000;

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

        private static void Counter(ref int counter)
        {
            counter++;
        }
    }
}