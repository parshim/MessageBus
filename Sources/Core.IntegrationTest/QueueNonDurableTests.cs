using System;
using System.Threading;
using System.Runtime.Serialization;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class QueueNonDurableTests
    {
        private const string NonDurableTestQueueSuffix = "NonDurableTests";

        [Test]
        public void Queue_MaxPriority_ShouldNotThrowException()
        {
            // Create a subscriber in order to create queue and bindings then dispose of it
            // queue and bindings should remain
            using (var bus = new MessageBus.Core.RabbitMQBus(c => { }))
            {
                // For incorrect data type e.g. byte instead of sbyte this test would fail
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseNonDurableNamedQueue("~TEMP").SetMaxPriority(8)))
                {
                    subscriber.Subscribe((Action<NonImportantData>)(p => { }));
                    subscriber.Open();
                    subscriber.Close();
                }
            }
        }

        [Test]
        public void Queue_NonDurableQueue_SubscribesAfterPublishing_MessagesShouldNotBeReceived()
        {
            // Create a subscriber in order to create queue and bindings then dispose of it
            // queue and bindings should remain
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => { }))
                {
                    subscriber.Subscribe((Action<NonImportantData>)(p => { }));
                    subscriber.Open();
                    subscriber.Close();
                }
            }

            NonImportantData data = new NonImportantData { Info = "Non-Valuable information" };

            // Dispatch message
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (IPublisher publisher = bus.CreatePublisher())
                {
                    publisher.Send(data);
                }
            }

            NonImportantData expected = null;
            ManualResetEvent ev = new ManualResetEvent(false);

            // Second bus subscribes to message after it was dispatched and should receive it
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => { }))
                {
                    subscriber.Subscribe<NonImportantData>(p =>
                    {
                        expected = p;

                        ev.Set();
                    });

                    subscriber.Open();

                    bool wait = ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeFalse();
                }
            }

            expected.Should().BeNull();
        }

        [Test]
        public void Queue_NonDurableQueue_TwoExclusiveSubscribers_BothShouldGetMessage()
        {
            NonImportantData data = new NonImportantData { Info = "Non-Valuable information" };

            NonImportantData expected1 = null;
            NonImportantData expected2 = null;
            ManualResetEvent ev1 = new ManualResetEvent(false);
            ManualResetEvent ev2 = new ManualResetEvent(false);
            bool wait1;
            bool wait2;
            IBus subscriberBus = null;
            ISubscriber subscriber1 = null, subscriber2 = null;

            try
            {
                subscriberBus = new MessageBus.Core.RabbitMQBus();
                subscriber1 = subscriberBus.CreateSubscriber(c => { });
                subscriber1.Subscribe<NonImportantData>(p =>
                {
                    expected1 = p;
                    ev1.Set();
                });
                subscriber1.Open();

                subscriber2 = subscriberBus.CreateSubscriber(c => { });
                subscriber2.Subscribe<NonImportantData>(p =>
                {
                    expected2 = p;
                    ev2.Set();
                });
                subscriber2.Open();

                // Dispatch message
                using (var bus = new MessageBus.Core.RabbitMQBus())
                {
                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(data);
                    }
                }
                
                wait1 = ev1.WaitOne(TimeSpan.FromMilliseconds(100));
                wait2 = ev2.WaitOne(TimeSpan.FromMilliseconds(100));
            }
            finally
            {
                subscriber2?.Dispose();
                subscriber1?.Dispose();
                subscriberBus?.Dispose();
            }

            wait1.Should().BeTrue();
            wait2.Should().BeTrue();

            expected1.Should().NotBeNull();
            expected1.ShouldBeEquivalentTo(data);

            expected2.Should().NotBeNull();
            expected2.ShouldBeEquivalentTo(data);
        }

        [Test]
        public void Queue_NonDurableQueue_TwoSharedQueueSubscribers_OneShouldGetMessage()
        {
            NonImportantData data = new NonImportantData { Info = "Non-Valuable information" };

            NonImportantData expected1 = null;
            NonImportantData expected2 = null;
            ManualResetEvent ev1 = new ManualResetEvent(false);
            ManualResetEvent ev2 = new ManualResetEvent(false);
            bool wait1;
            bool wait2;
            IBus subscriberBus = null;
            ISubscriber subscriber1 = null, subscriber2 = null;

            try
            {
                subscriberBus = new MessageBus.Core.RabbitMQBus();
                subscriber1 = subscriberBus.CreateSubscriber(c => c.UseNonDurableNamedQueue(NonDurableTestQueueSuffix));
                subscriber1.Subscribe<NonImportantData>(p =>
                {
                    expected1 = p;
                    ev1.Set();
                });
                subscriber1.Open();

                subscriber2 = subscriberBus.CreateSubscriber(c => c.UseNonDurableNamedQueue(NonDurableTestQueueSuffix));
                subscriber2.Subscribe<NonImportantData>(p =>
                {
                    expected2 = p;
                    ev2.Set();
                });
                subscriber2.Open();

                // Dispatch message
                using (var bus = new MessageBus.Core.RabbitMQBus())
                {
                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(data);
                    }
                }

                wait1 = ev1.WaitOne(TimeSpan.FromMilliseconds(100));
                wait2 = ev2.WaitOne(TimeSpan.FromMilliseconds(100));
            }
            finally
            {
                subscriber2?.Dispose();
                subscriber1?.Dispose();
                subscriberBus?.Dispose();
            }

            // Wait for reset events to timeout
            Thread.Sleep(500);

            wait1.ShouldBeEquivalentTo(!wait2);

            if (wait1)
            {
                expected2.Should().BeNull();
                expected1.Should().NotBeNull();
                expected1.ShouldBeEquivalentTo(data);
            }
            else
            {
                expected1.Should().BeNull();
                expected2.Should().NotBeNull();
                expected2.ShouldBeEquivalentTo(data);
            }
        }

        [DataContract]
        public class NonImportantData
        {
            [DataMember]
            public string Info { get; set; }
        }
    }
}
