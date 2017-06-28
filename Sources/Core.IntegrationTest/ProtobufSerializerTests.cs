using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.Protobuf;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class ProtobufSerializerTests
    {
        [Test]
        public void Protobuf_Serialize_Deserialize_Test()
        {
            var person = new Google.Protobuf.Examples.AddressBook.Person
            {
                Name = "Michael",
                Email = "parshim@gmail.com"
            };

            using (var bus = new RabbitMQBus())
            {
                using (var subscriber = bus.CreateSubscriber(c => c.AddProtobufSerializer().SetReceiveSelfPublish()))
                {
                    Google.Protobuf.Examples.AddressBook.Person actual = null;

                    ManualResetEvent ev = new ManualResetEvent(false);

                    subscriber.Subscribe<Google.Protobuf.Examples.AddressBook.Person>(p =>
                    {
                        actual = p;

                        ev.Set();
                    });

                    subscriber.Open();

                    using (var publisher = bus.CreatePublisher(c => c.UseProtobufSerializer()))
                    {
                        publisher.Send(person);
                    }

                    var res = ev.WaitOne(TimeSpan.FromSeconds(10));

                    Assert.True(res);
                    Assert.NotNull(actual);

                    Assert.True(actual.Equals(person));
                }
            }
        }
    }
}
