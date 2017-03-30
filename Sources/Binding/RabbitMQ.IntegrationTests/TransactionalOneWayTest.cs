using System;
using System.ServiceModel;
using System.Threading;
using System.Transactions;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Specialized;
using MessageBus.Binding.RabbitMQ;
using NUnit.Framework;
using RabbitMQ.IntegrationTests.ContractsAndServices;

namespace RabbitMQ.IntegrationTests
{
    [TestFixture]
    public class TransactionalOneWayTest
    {
        protected ServiceHost _host;
        protected ChannelFactory<IOneWayService> _channelFactory;
        protected readonly ManualResetEventSlim _ev = new ManualResetEventSlim();

        protected IOneWayService _processorFake = A.Fake<IOneWayService>();
        protected IOneWayService _errorProcessorFake = A.Fake<IOneWayService>();

        [TestFixtureTearDown]
        public virtual void TestCleanup()
        {
            _host.Close(TimeSpan.FromSeconds(2));

            _channelFactory.Close(TimeSpan.FromSeconds(2));

            _ev.Dispose();
        }

        /// <summary>
        /// amqp://username:password@localhost:5672/virtualhost/queueORexchange?routingKey=value
        ///  \_/   \_______________/ \_______/ \__/ \_________/ \_____________/ \______________/
        ///   |           |              |       |       |            |                 |                
        ///   |           |      broker hostname |       |            |         Specifies routing key value, may be empty
        ///   |           |                      |       |            |
        ///   |           |                      |  virtual host, should be absent if rabbit MQ not in cluster mode  
        ///   |           |                      |                    | 
        ///   |           |                      |                    |
        ///   |           |       node port, if absent 5672 is used   |
        ///   |           |                                           |
        ///   |  rabbit mq user info, if absent guest:guest is used   |
        ///   |                                                       |   
        ///   |                                 query name if used for receiving (service) side
        ///   |                                 exchange name if used for dispatching (client) side 
        ///scheme  
        /// name                                                    
        /// </summary>
        [TestFixtureSetUp]
        public void TestInitialize()
        {
            _processorFake = A.Fake<IOneWayService>();

            _host = new ServiceHost(new OneWayService(_processorFake, _errorProcessorFake));

            const string serviceAddress = "amqp://localhost/myQueue?routingKey=OneWayService";

            _host.AddServiceEndpoint(typeof(IOneWayService), new RabbitMQBinding
                {
                    AutoBindExchange = "amq.direct", // If not null, queue will be automatically binded to the exchange using provided routingKey (if any)
                    ExactlyOnce = true, // Transactional consumption,
                    OneWayOnly = true, // Use False only if calback communication required
                    //TTL = 1000, // Message time to leave in milliseconds
                    //PersistentDelivery = true // If true, every message will be written to disk on rabbitMQ broker side before dispatching to the destination(s)
                }, serviceAddress);

            _host.Open();


            const string clientAddress = "amqp://localhost/amq.direct?routingKey=OneWayService";

            _channelFactory = new ChannelFactory<IOneWayService>(new RabbitMQBinding
                {
                    OneWayOnly = true
                }, clientAddress);

            _channelFactory.Open();
        }


        [Test]
        public void RabbitMQBinding_TransactionalConsumption()
        {
            IOneWayService channel = _channelFactory.CreateChannel();

            Data data = new Data
                {
                    Id = 1,
                    Name = "Rabbit"
                };

            A.CallTo(() => _errorProcessorFake.Say(A<Data>._)).Throws(() => new Exception("Error while processing data")).Once();
            A.CallTo(() => _processorFake.Say(A<Data>.Ignored)).Invokes(() => _ev.Set());

            channel.Say(data);

            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            Assert.IsTrue(wait, "Service were not being invoked");

            A.CallTo(() => _processorFake.Say(A<Data>._)).WhenArgumentsMatch(collection =>
                {
                    data.ShouldBeEquivalentTo(collection[0]);

                    return true;
                }).MustHaveHappened();
        }
        
        [Test]
        public void RabbitMQBinding_TransactionalDispatching_MessagesRollbacked()
        {
            IOneWayService channel = _channelFactory.CreateChannel();

            Data data = new Data
                {
                    Id = 1,
                    Name = "Rabbit"
                };

            A.CallTo(() => _errorProcessorFake.Say(A<Data>._)).DoesNothing();
            A.CallTo(() => _processorFake.Say(A<Data>.Ignored)).Invokes(() => _ev.Set());

            using (new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                channel.Say(data);

                channel.Say(data);

                // Do not call transaction complete
            }
            
            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            Assert.IsFalse(wait, "Service should not be invoked");

            A.CallTo(() => _processorFake.Say(A<Data>._)).MustNotHaveHappened();
        }
        
        [Test]
        public void RabbitMQBinding_TransactionalDispatching_MessagesCommited()
        {
            IOneWayService channel = _channelFactory.CreateChannel();

            Data data = new Data
                {
                    Id = 1,
                    Name = "Rabbit"
                };

            A.CallTo(() => _errorProcessorFake.Say(A<Data>._)).DoesNothing();
            A.CallTo(() => _processorFake.Say(A<Data>.Ignored)).Invokes(() => _ev.Set());

            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                channel.Say(data);

                // Complete the transaction
                transaction.Complete();
            }

            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            Assert.IsTrue(wait, "Service were not being invoked");

            A.CallTo(() => _processorFake.Say(A<Data>._)).WhenArgumentsMatch(collection =>
            {
                data.ShouldBeEquivalentTo(collection[0]);

                return true;
            }).MustHaveHappened();
        }
        
        [Test]
        public void RabbitMQBinding_TransactionalDispatching_ExceptionIfTransactionalChannelUsedOutOfTheTransactionScope()
        {
            IOneWayService channel = _channelFactory.CreateChannel();

            Data data = new Data
                {
                    Id = 1,
                    Name = "Rabbit"
                };

            A.CallTo(() => _errorProcessorFake.Say(A<Data>._)).DoesNothing();
            A.CallTo(() => _processorFake.Say(A<Data>.Ignored)).Invokes(() => _ev.Set());

            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                channel.Say(data);

                // Complete the transaction
                transaction.Complete();
            }

            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            Assert.IsTrue(wait, "Service were not being invoked");

            // Same channel instance can't be used outsode transaction scope
            ((Action)(() => channel.Say(new Data()))).ShouldThrow<FaultException>();
        }
    }
}