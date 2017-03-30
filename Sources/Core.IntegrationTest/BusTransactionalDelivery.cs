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

                    routeManager.QueueBindMessage<TransactionalPerson>(QueueName);
                    routeManager.QueueBindMessage<TransactionalPerson>(DeadLetterQueueName, "amq.direct", "fail");
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
                    routeManager.DeleteQueue(DeadLetterQueueName);
                }
            }
        }

        [Test]
        public void MessageProcessFailedOnce_CheckMessageRedelivered()
        {
            using (RabbitMQBus entityA = new RabbitMQBus(), entityB = new RabbitMQBus())
            {
                TransactionalPerson message = new TransactionalPerson
                {
                    Id = 5
                };

                ManualResetEvent ev = new ManualResetEvent(false);

                int counter = 0;

                using (ISubscriber subscriberA = entityA.CreateSubscriber(c => c.UseTransactionalDelivery()))
                {
                    subscriberA.Subscribe((Action<TransactionalPerson>)(d =>
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
                TransactionalPerson message = new TransactionalPerson
                    {
                        Id = 5
                    };

                ManualResetEvent ev = new ManualResetEvent(false);

                int counter = 0;

                BusMessage<TransactionalPerson> actual = null;

                using (ISubscriber subscriberA = entityA.CreateSubscriber(c => c.UseDurableQueue(QueueName).UseTransactionalDelivery()))
                {
                    subscriberA.Subscribe((Action<TransactionalPerson>) (d =>
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
                        deadLetterSubscriber.Subscribe<TransactionalPerson>(m =>
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

                        counter.Should().BeGreaterOrEqualTo(expected);
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
        
        [Test]
        public void MessageProcessFailedOnce_RejectMessageUsingExceptionFilter_CheckMessageDeliveredToDeadLetterQ()
        {
            using (RabbitMQBus entityA = new RabbitMQBus(), entityB = new RabbitMQBus(), entityC = new RabbitMQBus(c => c.UseConnectionString("amqp://localhost/amq.direct")))
            {
                TransactionalPerson message = new TransactionalPerson
                {
                        Id = 5
                    };

                ManualResetEvent ev = new ManualResetEvent(false);

                int counter = 0;

                BusMessage<TransactionalPerson> actual = null;

                using (ISubscriber subscriberA = entityA.CreateSubscriber(c => c.UseDurableQueue(QueueName).UseTransactionalDelivery(new MyFilter())))
                {
                    subscriberA.Subscribe((Action<TransactionalPerson>) (d =>
                    {
                        counter++;

                        throw new Exception();
                    }));

                    subscriberA.Open();

                    using (ISubscriber deadLetterSubscriber = entityC.CreateSubscriber(c => c.UseDurableQueue(DeadLetterQueueName)))
                    {
                        deadLetterSubscriber.Subscribe<TransactionalPerson>(m =>
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

                        counter.Should().BeGreaterOrEqualTo(expected);
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

        // Created a specific class for these tests to prevent bindings from other tests from interfering
        public class TransactionalPerson : Person
        {
        }
    }

    public class MyFilter : IExceptionFilter
    {
        public bool Filter(Exception exception, RawBusMessage message, bool redelivered, ulong deliveryTag)
        {
            return !redelivered;
        }
    }
}
