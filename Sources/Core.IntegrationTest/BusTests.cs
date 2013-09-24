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
        public void Test_Send_Recieve()
        {
            using (IBus entityA = new Bus(), entityB = new Bus())
            {
                Data messageA = new Data
                    {
                        Id = 5
                    };
                
                Data messageB = new Data
                    {
                        Id = 25
                    };

                List<Data> received = new List<Data>();

                ManualResetEvent ev1 = new ManualResetEvent(false), ev2 = new ManualResetEvent(false);

                entityA.Register<Data>(received.Add);

                entityA.Register<OK>(data => ev1.Set());
                
                entityB.Register<OK>(data => ev2.Set());

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

                bool waitOne = ev1.WaitOne(TimeSpan.FromSeconds(10)) && ev2.WaitOne(TimeSpan.FromSeconds(10));

                waitOne.Should().BeTrue("Message not received");

                received.Should().ContainInOrder(messageA, messageB);
            }
        }
    }

    [DataContract]
    public class Data
    {
        [DataMember]
        public int Id { get; set; }

        protected bool Equals(Data other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Data) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    [DataContract]
    public class OK
    {
        
    }
}
