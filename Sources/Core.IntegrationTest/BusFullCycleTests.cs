using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusFullCycleTests
    {
        [Test]
        public void RabbitMQBus_Dialog_WithOrderedDelivery_AllSubscriptionTypes()
        {
            using (MessageBus.Core.RabbitMQBus entityA = new MessageBus.Core.RabbitMQBus(), entityB = new MessageBus.Core.RabbitMQBus())
            {
                Data messageA = new Person
                    {
                        Id = 5
                    };
                
                Data messageB = new Car
                    {
                        Number = "39847239847"
                    };

                List<Data> received = new List<Data>();

                ManualResetEvent ev1 = new ManualResetEvent(false), ev2 = new ManualResetEvent(false);

                using (ISubscriber subscriberA = entityA.CreateSubscriber(), subscriberB = entityB.CreateSubscriber())
                {
                    subscriberA.Subscribe<Data>(received.Add, true);

                    subscriberA.Subscribe(typeof(OK), (Action<object>) (data => ev1.Set()));

                    subscriberB.Subscribe((Action<OK>) (data => ev2.Set()));

                    subscriberA.Open();
                    subscriberB.Open();

                    using (IPublisher publisher = entityB.CreatePublisher())
                    {
                        publisher.Send(messageA);

                        publisher.Send(messageB);

                        publisher.Send(new OK());
                    }

                    using (IPublisher publisher = entityA.CreatePublisher())
                    {
                        publisher.Send(new OK());
                    }

                    bool waitOne = ev1.WaitOne(TimeSpan.FromSeconds(20)) && ev2.WaitOne(TimeSpan.FromSeconds(20));

                    waitOne.Should().BeTrue("Message not received");

                    received.Should().ContainInOrder(messageA, messageB);
                }
            }
        }

    }

}
