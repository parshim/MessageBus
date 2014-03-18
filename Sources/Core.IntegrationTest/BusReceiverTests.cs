using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusReceiverTests
    {
        [TestMethod]
        public void Bus_PublishedMessage_ReceiveSelfPublishIsFalse_ShouldNotArriveToReceiver()
        {
            using (RabbitMQBus bus = new RabbitMQBus())
            {
                using (IReceiver receiver = bus.CreateRecevier())
                {
                    receiver.Subscribe<OK>(receiveSelfPublish: false);

                    receiver.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new OK());
                    }

                    OK receive = receiver.Receive<OK>();

                    receive.Should()
                        .BeNull("Message should not arrive from publisher to subscriber within same bus instance");
                }
            }
        }

        [TestMethod]
        public void Bus_PublishedMessage_ReceiveSelfPublishIsTrue_ShouldArriveToReceiver()
        {
            using (var bus = new RabbitMQBus())
            {
                using (IReceiver receiver = bus.CreateRecevier())
                {
                    receiver.Subscribe<OK>(receiveSelfPublish: true);

                    receiver.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new OK());
                    }

                    OK receive = receiver.Receive<OK>();

                    receive.Should()
                        .NotBeNull("Message should not arrive from publisher to subscriber within same bus instance");
                }
            }
        }

        [TestMethod]
        public void Bus_PublishedMessage_EachReceiverGetsMessageCopy()
        {
            Person actual = new Person
                {
                    Id = 5
                };

            using (RabbitMQBus busA = new RabbitMQBus(), busB = new RabbitMQBus(), busC = new RabbitMQBus())
            {
                using (IReceiver receiverB1 = busB.CreateRecevier(), receiverB2 = busB.CreateRecevier(), receiverC1 = busC.CreateRecevier())
                {
                    receiverB1.Subscribe<Person>();
                    receiverB2.Subscribe<Person>();
                    receiverC1.Subscribe<Person>();

                    receiverB1.Open();
                    receiverB2.Open();
                    receiverC1.Open();

                    using (IPublisher publisher = busA.CreatePublisher())
                    {
                        publisher.Send(actual);
                    }

                    Person b1 = receiverB1.Receive<Person>();
                    Person b2 = receiverB2.Receive<Person>();
                    Person c1 = receiverC1.Receive<Person>();

                    b1.Should().NotBeNull();
                    b2.Should().NotBeNull();
                    c1.Should().NotBeNull();

                    b1.ShouldBeEquivalentTo(actual);
                    b2.ShouldBeEquivalentTo(actual);
                    c1.ShouldBeEquivalentTo(actual);

                }
            }
        }

        [TestMethod]
        public void Bus_PublishedMessage_FilterByHeader()
        {
            Person actualT1 = new Person
                {
                    Id = 5
                };

            Person actualT2 = new Person
                {
                    Id = 15
                };

            const string header = "type";

            using (RabbitMQBus busA = new RabbitMQBus(), busB = new RabbitMQBus(), busC = new RabbitMQBus())
            {
                using (IReceiver receiverB1 = busB.CreateRecevier(), receiverB2 = busB.CreateRecevier(), receiverC1 = busC.CreateRecevier())
                {
                    receiverB1.Subscribe<Person>(filter: new[] { new BusHeader { Name = header, Value = "T1" } });
                    receiverB2.Subscribe<Person>(filter: new[] { new BusHeader { Name = header, Value = "T2" } });
                    receiverC1.Subscribe<Person>(filter: new[] { new BusHeader { Name = header, Value = "T1" } });

                    receiverB1.Open();
                    receiverB2.Open();
                    receiverC1.Open();

                    using (IPublisher publisher = busA.CreatePublisher())
                    {
                        BusMessage<Person> m1 = new BusMessage<Person> { Data = actualT1 };
                        m1.Headers.Add(new BusHeader { Name = header, Value = "T1" });

                        publisher.Send(m1);

                        BusMessage<Person> m2 = new BusMessage<Person> { Data = actualT2 };
                        m2.Headers.Add(new BusHeader { Name = header, Value = "T2" });

                        publisher.Send(m2);
                    }

                    Person b1 = receiverB1.Receive<Person>();
                    Person b2 = receiverB2.Receive<Person>();
                    Person c1 = receiverC1.Receive<Person>();

                    b1.Should().NotBeNull();
                    b2.Should().NotBeNull();
                    c1.Should().NotBeNull();

                    b1.ShouldBeEquivalentTo(actualT1);
                    b2.ShouldBeEquivalentTo(actualT2);
                    c1.ShouldBeEquivalentTo(actualT1);
                }
            }
        }
    }
}