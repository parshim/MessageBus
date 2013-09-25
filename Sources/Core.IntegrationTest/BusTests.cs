using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusTests
    {
        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanup()
        {
        }
        
        [TestMethod]
        public void Bus_Dialog_WithOrderedDelivery_AllSubscriptionTypes()
        {
            using (IBus entityA = new Bus(), entityB = new Bus())
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
                    subscriberA.SubscribeHierarchy<Data>(received.Add);

                    subscriberA.Subscribe(typeof(OK), data => ev1.Set());

                    subscriberB.Subscribe<OK>(data => ev2.Set());

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

        [TestMethod]
        public void Bus_SendReceive_1000Messages()
        {
            using (IBus entityA = new Bus(), entityB = new Bus())
            {
                Data messageA = new Person
                {
                    Id = 5
                };
                
                ManualResetEvent ev1 = new ManualResetEvent(false);

                int counter = 0;

                using (ISubscriber subscriberA = entityA.CreateSubscriber())
                {
                    subscriberA.SubscribeHierarchy<Data>(d => counter++);

                    subscriberA.Subscribe(typeof(OK), data => ev1.Set());

                    using (IPublisher publisher = entityB.CreatePublisher())
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            publisher.Send(messageA);
                        }
                        
                        publisher.Send(new OK());
                    }
                    
                    bool waitOne = ev1.WaitOne(TimeSpan.FromSeconds(20));

                    waitOne.Should().BeTrue("Message not received");

                    counter.Should().Be(1000);
                }
            }
        }
    }


    [DataContract]
    public class Data
    {
        
    }

    [DataContract]
    public class Person : Data
    {
        [DataMember]
        public int Id { get; set; }

        protected bool Equals(Person other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Person)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
    
    [DataContract]
    public class Car : Data
    {
        [DataMember]
        public string Number { get; set; }

        protected bool Equals(Car other)
        {
            return string.Equals(Number, other.Number);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Car) obj);
        }

        public override int GetHashCode()
        {
            return (Number != null ? Number.GetHashCode() : 0);
        }
    }

    [DataContract]
    public class OK
    {
        
    }
}
