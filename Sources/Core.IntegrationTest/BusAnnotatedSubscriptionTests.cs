using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusAnnotatedSubscriptionTests
    {
        [Test]
        public void Bus_SimpleSubscription_CallReceivedOnImplementationInstace()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            SimpleImplementation implementation = new SimpleImplementation(ev);

            using (IBus bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation, c => c.SetReceiveSelfPublish()))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "Value"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                        waitOne.Should().BeTrue();

                        person.ShouldBeEquivalentTo(implementation.Person);
                    }
                }
            }
        }
        
        [Test]
        public void Bus_SubscriptionWithFilter_CallFiltered()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            FilterImplementation implementation = new FilterImplementation(ev);

            using (IBus bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation, c => c.SetReceiveSelfPublish()))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "WrongValue"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                        waitOne.Should().BeFalse();
                    }
                }
            }
        }
        
        [Test]
        public void Bus_SubscriptionWithFilter_CallReceived()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            FilterImplementation implementation = new FilterImplementation(ev);

            using (IBus bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation, c => c.SetReceiveSelfPublish()))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "RightValue"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                        waitOne.Should().BeTrue();

                        person.ShouldBeEquivalentTo(implementation.Person);
                    }
                }
            }
        }
        
        [Test]
        public void Bus_MessageBaseSubscription_CallReceived()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            MessageBasedImplementation implementation = new MessageBasedImplementation(ev);

            using (IBus bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation, c => c.SetReceiveSelfPublish()))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "RightValue"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5000));

                        waitOne.Should().BeTrue();

                        busMessage.ShouldBeEquivalentTo(implementation.Message,
                                                        options =>
                                                        options.Excluding(message => message.BusId)
                                                               .Excluding(message => message.Sent));
                    }
                }
            }
        }
    }

    [Subscribtion]
    public class SimpleImplementation
    {
        private readonly ManualResetEvent _ev;
        private Person _person;

        public SimpleImplementation(ManualResetEvent ev)
        {
            _ev = ev;
        }

        public Person Person
        {
            get { return _person; }
        }

        [MessageSubscription]
        public void ProcessPerson(Person person)
        {
            _person = person;

            _ev.Set();
        }
    }
    
    [Subscribtion]
    public class MessageBasedImplementation
    {
        private readonly ManualResetEvent _ev;
        private BusMessage<Person> _message;

        public MessageBasedImplementation(ManualResetEvent ev)
        {
            _ev = ev;
        }

        public BusMessage<Person> Message
        {
            get { return _message; }
        }

        [MessageSubscription]
        public void ProcessPerson(BusMessage<Person> message)
        {
            _message = message;

            _ev.Set();
        }
    }
    
    [Subscribtion]
    public class FilterImplementation
    {
        private readonly ManualResetEvent _ev;
        private Person _person;

        public FilterImplementation(ManualResetEvent ev)
        {
            _ev = ev;
        }

        public Person Person
        {
            get { return _person; }
        }

        [MessageSubscription]
        public void ProcessPerson([HeaderFilter("Header", "RightValue")]Person person)
        {
            _person = person;

            _ev.Set();
        }
    }
}
