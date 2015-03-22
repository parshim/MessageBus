using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;

using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Core.IntegrationTest
{
    [TestClass]
    public class ErrorSubscriberTest : IErrorSubscriber
    {
        private RawBusMessage _busMessage;
        private Exception _exception;

        private readonly ManualResetEvent _ev = new ManualResetEvent(false);

        [TestMethod]
        public void Bus_ErrorSubscriber_MessageFilteredOut()
        {
            const string busId = "MyBus";

            using (IBus bus = new MessageBus.Core.RabbitMQBus(busId))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseErrorSubscriber(this)))
                {
                    subscriber.Subscribe((OK ok) => Trace.WriteLine("Received"));

                    subscriber.Open();

                    OK expectation = new OK();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(expectation);
                    }

                    bool wait = _ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue();

                    _busMessage.Name.Should().Be("OK");
                    _busMessage.Namespace.Should().Be("bus.test.org");
                    _busMessage.Data.Should().BeOfType<OK>();
                    _busMessage.BusId.Should().Be(busId);
                }
            }
        }
        
        [TestMethod]
        public void Bus_ErrorSubscriber_MessageDeserializedException()
        {
            const string busId = "MyBus";

            using (IBus bus = new MessageBus.Core.RabbitMQBus(busId))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseErrorSubscriber(this).SetReceiveSelfPublish()))
                {
                    subscriber.Subscribe(delegate(ContractToReceive ok) { });
                    
                    subscriber.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new ContractToSend { Data = 4 });
                    }

                    bool wait = _ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue();

                    _busMessage.Name.Should().Be("Data");
                    _busMessage.Namespace.Should().Be("bus.error.test.org");
                    _busMessage.Data.GetType().Should().Be(typeof(byte[]));
                    _busMessage.BusId.Should().Be(busId);
                    _exception.Should().BeOfType<JsonSerializationException>();
                }
            }
        }
        
        [TestMethod]
        public void Bus_ErrorSubscriber_MessageDispatchException()
        {
            const string busId = "MyBus";

            Exception ex = new Exception("My process error");

            using (IBus bus = new MessageBus.Core.RabbitMQBus(busId))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseErrorSubscriber(this).SetReceiveSelfPublish()))
                {
                    subscriber.Subscribe(delegate(ContractToSend ok) { throw ex; });
                    
                    subscriber.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new ContractToSend { Data = 4 });
                    }

                    bool wait = _ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue();

                    _busMessage.Name.Should().Be("Data");
                    _busMessage.Namespace.Should().Be("bus.error.test.org");
                    _busMessage.Data.Should().BeOfType<ContractToSend>();
                    _busMessage.BusId.Should().Be(busId);
                    _exception.Should().BeSameAs(ex);
                }
            }
        }
        
        [TestMethod]
        public void Bus_ErrorSubscriber_UnregisteredMessageShouldNotArrive()
        {
            const string busId = "MyBus";

            using (IBus bus = new MessageBus.Core.RabbitMQBus(busId))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseErrorSubscriber(this)))
                {
                    subscriber.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(new ContractToSend { Data = 4 });
                    }

                    bool wait = _ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeFalse();
                    _busMessage.Should().BeNull();
                }
            }
        }

        public void MessageDeserializeException(RawBusMessage busMessage, Exception exception)
        {
            _busMessage = busMessage;
            _exception = exception;

            _ev.Set();
        }

        public void MessageDispatchException(RawBusMessage busMessage, Exception exception)
        {
            _busMessage = busMessage;
            _exception = exception;

            _ev.Set();
        }

        public void MessageFilteredOut(RawBusMessage busMessage)
        {
            _busMessage = busMessage;

            _ev.Set();
        }

        public void UnregisteredMessageArrived(RawBusMessage busMessage)
        {
            _busMessage = busMessage;

            _ev.Set();
        }

        [DataContract(Name = "Data", Namespace = "bus.error.test.org")]
        public class ContractToSend
        {
            [DataMember]
            public int Data { get; set; }
        }

        [DataContract(Name = "Data", Namespace = "bus.error.test.org")]
        public class ContractToReceive
        {
            [DataMember]
            public Person Data { get; set; }
        }
    }
}