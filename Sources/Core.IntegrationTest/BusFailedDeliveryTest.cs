using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusFailedDeliveryTest : IPublishingErrorHandler
    {
        private readonly ManualResetEvent _ev = new ManualResetEvent(false);

        private int _errorCode;
        private string _text;
        private RawBusMessage _message;
        
        [Test]
        public void Bus_UndeliverableMessages_ReturnedToSubscriber()
        {
            using (IBus bus = new MessageBus.Core.RabbitMQBus())
            {
                using (IPublisher publisher = bus.CreatePublisher(c => c.SetMandatoryDelivery().UseErrorHandler(this)))
                {
                    UndeliverablePerson person = new UndeliverablePerson { Id = 5};

                    BusMessage<UndeliverablePerson> busMessage = new BusMessage<UndeliverablePerson>
                        {
                            Data = person
                        };

                    busMessage.Headers.Add(new BusHeader
                        {
                            Name = "Header",
                            Value = "Value"
                        });

                    publisher.Send(busMessage);

                    bool waitOne = _ev.WaitOne(TimeSpan.FromSeconds(5));

                    waitOne.Should().BeTrue();

                    _errorCode.Should().Be(312);
                    _text.Should().Be("NO_ROUTE");

                    _message.BusId.Should().Be(bus.BusId);
                    _message.Name.Should().Be("UndeliverablePerson");
                    _message.Sent.Should().BeCloseTo(DateTime.Now, 2000);
                    _message.Data.Should().BeOfType<UndeliverablePerson>();

                    _message.Headers.OfType<BusHeader>().Should().OnlyContain(header => header.Name == "Header" && header.Value == "Value");

                    person.ShouldBeEquivalentTo(_message.Data);
                }
            }
        }

        // Created a specific class for these tests to prevent bindings from other tests from interfering
        public class UndeliverablePerson : Person
        {
        }

        public void DeliveryFailed(int errorCode, string text, RawBusMessage message)
        {
            _errorCode = errorCode;
            _text = text;
            _message = message;

            _ev.Set();
        }
    }
}