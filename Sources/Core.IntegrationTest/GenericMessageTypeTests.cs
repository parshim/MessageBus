using System;
using System.Threading;
using System.Runtime.Serialization;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;
using RabbitMQ.Client.Exceptions;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class GenericMessageTypeTests
    {
        [Test]
        public void Queue_NonDurableQueue_TwoExclusiveSubscribers_BothShouldGetMessage()
        {
            var receivedMessages = new int[2];
            for (int subscriberNum = 0; subscriberNum < receivedMessages.GetLength(0); subscriberNum++)
                receivedMessages[subscriberNum] = 0;

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
                subscriber1.Subscribe((GenericMessage<Specific1> p) =>
                {
                    Interlocked.Increment(ref receivedMessages[0]);
                    ev1.Set();
                });
                subscriber1.Open();

                subscriber2 = subscriberBus.CreateSubscriber(c => { });
                subscriber2.Subscribe((GenericMessage<Specific2> p) =>
                {
                    Interlocked.Increment(ref receivedMessages[1]);
                    ev2.Set();
                });
                subscriber2.Open();

                // Dispatch message
                using (var bus = new MessageBus.Core.RabbitMQBus())
                {
                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new GenericMessage<Specific2> { RequestReference = Guid.NewGuid() });
                        publisher.Send(new GenericMessage<Specific1> { RequestReference = Guid.NewGuid() });
                        publisher.Send(new GenericMessage<Specific2> { RequestReference = Guid.NewGuid() });
                        publisher.Send(new GenericMessage<Specific2> { RequestReference = Guid.NewGuid() });
                        publisher.Send(new GenericMessage<Specific1> { RequestReference = Guid.NewGuid() });
                        publisher.Send(new GenericMessage<Specific2> { RequestReference = Guid.NewGuid() });
                        publisher.Send(new GenericMessage<Specific2> { RequestReference = Guid.NewGuid() });
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

            receivedMessages[0].ShouldBeEquivalentTo(2);

            receivedMessages[1].ShouldBeEquivalentTo(5);
        }

        public class GenericMessage<TSpecific>
        {
            public Guid RequestReference { get; set; }
        }

        public class Specific1
        {
        }

        public class Specific2
        {
        }
    }
}
