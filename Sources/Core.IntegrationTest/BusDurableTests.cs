using System;
using System.Threading;
using System.Runtime.Serialization;
using FluentAssertions;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusDurableTests
    {
        [TestMethod]
        public void Bus_DurableBus_SubscribesAfterPublishing_MessagesShouldBeReceived()
        {
            ImportiantData data = new ImportiantData { Info = "Valuable information" };

            // First bus ensures that test queue is created with appropriate bindings
            const string queueName = "DurableTestQueue";

            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseDurableQueue(queueName)))
                {
                    subscriber.Subscribe(new Action<ImportiantData>(d => { }));
                }
            }

            // Dispatch message
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (IPublisher publisher = bus.CreatePublisher())
                {
                    publisher.Send(data);
                }
            }

            ImportiantData expected = null;
            ManualResetEvent ev = new ManualResetEvent(false);

            // Second bus subscribes to message after it was dispatched and should receive it
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseDurableQueue(queueName)))
                {
                    subscriber.Subscribe<ImportiantData>(p =>
                        {
                            expected = p;

                            ev.Set();
                        });

                    subscriber.Open();

                    bool wait = ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue();
                }
            }

            expected.Should().NotBeNull();

            expected.ShouldBeEquivalentTo(data);
        }
    }

    [DataContract]
    public class ImportiantData
    {
        [DataMember]
        public string Info { get; set; }
    }
}
