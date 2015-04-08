using System;
using System.Threading;
using FakeItEasy;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;

using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusMessageFormatTest
    {
        [Test]
        public void UseSoapFormat_MessageSent_Received()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            using (IBus busA = new RabbitMQBus(), busB = new RabbitMQBus())
            {
                Person person = new Person
                {
                    Id = 5
                };

                Person actual = null;

                using (ISubscriber subscriber = busA.CreateSubscriber())
                {
                    subscriber.Subscribe((Action<Person>) (p =>
                    {
                        actual = p;

                        ev.Set();
                    }));

                    subscriber.Open();
                    
                    using (IPublisher publisher = busB.CreatePublisher(c => c.UseSoapSerializer()))
                    {
                        publisher.Send(person);
                    }

                    ev.WaitOne(TimeSpan.FromSeconds(5));

                    actual.ShouldBeEquivalentTo(person);
                }
            }
        }
        
        [Test]
        public void UseCustomFormat_MessageSent_Received()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            using (IBus busA = new RabbitMQBus(), busB = new RabbitMQBus())
            {
                Person person = new Person
                {
                    Id = 5
                };

                Person actual = null;

                ISerializer serializer = A.Fake<ISerializer>();

                A.CallTo(() => serializer.Serialize(A<RawBusMessage>._)).Returns(new byte[0]);
                A.CallTo(() => serializer.Deserialize(new DataContractKey(typeof(Person).Name, typeof(Person).Namespace), typeof(Person), A<byte[]>._)).Returns(person);
                A.CallTo(() => serializer.ContentType).Returns("Custom");
                
                using (ISubscriber subscriber = busA.CreateSubscriber(c => c.AddCustomSerializer(serializer)))
                {
                    subscriber.Subscribe((Action<Person>) (p =>
                    {
                        actual = p;

                        ev.Set();
                    }));

                    subscriber.Open();
                    
                    using (IPublisher publisher = busB.CreatePublisher(c => c.UseCustomSerializer(serializer)))
                    {
                        publisher.Send(new Person());
                    }

                    ev.WaitOne(TimeSpan.FromSeconds(5));

                    actual.ShouldBeEquivalentTo(person);
                }

                A.CallTo(() => serializer.Serialize(A<RawBusMessage>._)).MustHaveHappened();
                A.CallTo(() => serializer.Deserialize(new DataContractKey(typeof(Person).Name, typeof(Person).Namespace), typeof(Person), A<byte[]>._)).MustHaveHappened();
            }
        }
    }
}
