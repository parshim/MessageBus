using System;
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
        [Test]
        public void MessageProcessFailed_CheckMessageRedelivered()
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
    }
}
