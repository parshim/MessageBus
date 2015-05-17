using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusTransactionalDelivery
    {
        private const string QueueName = "TestFailProcess";
        private const string DeadLetterQueueName = "TestDeadLetter";

        [TestFixtureSetUp]
        public void Initialize()
        {
            using (var bus = new RabbitMQBus())
            {
                using (IRouteManager routeManager = bus.CreateRouteManager())
                {
                    routeManager.CreateQueue(QueueName, true, false, new CreateQueueSettings
                    {
                        DeadLetterExchange = "amq.direct",
                        DeadLetterRoutingKey = "fail"
                    });

                    routeManager.CreateQueue(DeadLetterQueueName, true, false, CreateQueueSettings.Default);

                    routeManager.QueueBindMessage<Person>(QueueName);
                    routeManager.QueueBindMessage<Person>(DeadLetterQueueName, "amq.direct", "fail");
                }
            }
        }

        [TestFixtureTearDown]
        public void Clean()
        {
            using (var bus = new RabbitMQBus())
            {
                using (IRouteManager routeManager = bus.CreateRouteManager())
                {
                    routeManager.DeleteQueue(QueueName);
                }
            }
        }

        [Test]
        public void MessageProcessFailedOnce_CheckMessageRedelivered()
        {
            using (RabbitMQBus entityA = new RabbitMQBus(), entityB = new RabbitMQBus())
            {
                Person message = new Person
                {
                    Id = 5
                };

                ManualResetEvent ev = new ManualResetEvent(false);

                int counter = 0;

                using (ISubscriber subscriberA = entityA.CreateSubscriber(c => c.UseTransactionalDelivery()))
                {
                    subscriberA.Subscribe((Action<Person>)(d =>
                    {
                        counter++;

                        if (counter == 1)
                        {
                            throw new Exception();
                        }
                    }));

                    subscriberA.Open();

                    const int expected = 2;

                    using (IPublisher publisher = entityB.CreatePublisher())
                    {
                        publisher.Send(message);
                    }

                    ev.WaitOne(TimeSpan.FromSeconds(5));

                    counter.Should().Be(expected);
                }
            }
        }

        [Test]
        public void MessageProcessFailedOnce_RejectMessage_CheckMessageDeliveredToDeadLetterQ()
        {
            using (RabbitMQBus entityA = new RabbitMQBus(), entityB = new RabbitMQBus(), entityC = new RabbitMQBus(c => c.UseConnectionString("amqp://localhost/amq.direct")))
            {
                Person message = new Person
                    {
                        Id = 5
                    };

                ManualResetEvent ev = new ManualResetEvent(false);

                int counter = 0;

                BusMessage<Person> actual = null;

                using (ISubscriber subscriberA = entityA.CreateSubscriber(c => c.UseDurableQueue(QueueName).UseTransactionalDelivery()))
                {
                    subscriberA.Subscribe((Action<Person>) (d =>
                    {
                        counter++;

                        if (counter == 1)
                        {
                            throw new Exception();
                        }

                        throw new RejectMessageException();
                    }));

                    subscriberA.Open();

                    using (ISubscriber deadLetterSubscriber = entityC.CreateSubscriber(c => c.UseDurableQueue(DeadLetterQueueName)))
                    {
                        deadLetterSubscriber.Subscribe<Person>(m =>
                        {
                            actual = m;
                            
                            ev.Set();
                        });

                        deadLetterSubscriber.Open();

                        const int expected = 2;

                        using (IPublisher publisher = entityB.CreatePublisher())
                        {
                            publisher.Send(message);
                        }

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(50));

                        counter.Should().Be(expected);
                        waitOne.Should().BeTrue();
                        actual.Data.ShouldBeEquivalentTo(message);

                        XDeadHeader xDeadHeader = actual.Headers.OfType<XDeadHeader>().First();

                        xDeadHeader.Exchange.Should().Be("amq.headers");
                        xDeadHeader.Reason.Should().Be("rejected");
                        xDeadHeader.Queue.Should().Be(QueueName);
                    }
                }
            
            }
        }
    }
}
