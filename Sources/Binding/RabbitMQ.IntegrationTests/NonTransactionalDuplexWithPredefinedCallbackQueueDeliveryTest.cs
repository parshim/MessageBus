using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using FakeItEasy;
using FluentAssertions;
using MessageBus.Binding.RabbitMQ;
using NUnit.Framework;
using RabbitMQ.IntegrationTests.ContractsAndServices;

namespace RabbitMQ.IntegrationTests
{
    [TestFixture]
    public class NonTransactionalDuplexWithPredefinedCallbackQueueDeliveryTest
    {
        Data _replyData;
        ServiceHost _host;
        ChannelFactory<IDuplexService> _channelFactory;
        readonly ManualResetEventSlim _ev = new ManualResetEventSlim();

        IDuplexService _processorFake = A.Fake<IDuplexService>();
        IDuplexService _errorProcessorFake = A.Fake<IDuplexService>();
        IDuplexService _callbackFake = A.Fake<IDuplexService>();

        [TestFixtureTearDown]
        public void TestCleanup()
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
            _replyData = new Data
                {
                    Id = 2,
                    Name = "Reply"
                };

            _host = new ServiceHost(new DuplexService(_processorFake, _replyData));

            const string serviceAddress = "amqp://localhost/myDuplexQueue?routingKey=DuplexService";

            ServiceEndpoint endpoint = _host.AddServiceEndpoint(typeof (IDuplexService), new RabbitMQBinding
                {
                    AutoBindExchange = "amq.direct", // If not null, queue will be automatically binded to the exchange using provided routingKey (if any)
                    ExactlyOnce = false, // Non-transactional consumption,
                    OneWayOnly = false, // Use False only if calback communication required
                    //TTL = 1000, // Message time to leave in miliseconds
                    //PersistentDelivery = true // If true, every message will be written to disk on rabbitMQ broker side before dispatching to the destination(s)
                }, serviceAddress);

            // Required behaviour for duplex comunication
            endpoint.Behaviors.Add(new ReplyToBehavior());

            _host.Open();


            const string clientAddress = "amqp://localhost/amq.direct?routingKey=DuplexService";

            _channelFactory = new DuplexChannelFactory<IDuplexService>(new InstanceContext(_callbackFake), new RabbitMQBinding
                {
                    OneWayOnly = false,
                    AutoBindExchange = "amq.direct",
                    ReplyToExchange = new Uri("amqp://localhost/amq.direct?routingKey=DuplexCallbackService"),
                    ReplyToQueue = "myCallBackQueue?routingKey=DuplexCallbackService"
                }, clientAddress);

            _channelFactory.Open();
        }


        [Test]
        public void RabbitMQBinding_NonTransactionalDuplexWithPredefinedCallbackQueueDelivery()
        {
            IDuplexService channel = _channelFactory.CreateChannel();

            Data data = new Data
                {
                    Id = 1,
                    Name = "Rabbit"
                };

            A.CallTo(_errorProcessorFake).DoesNothing();
            A.CallTo(() => _callbackFake.Say(A<Data>.Ignored)).Invokes(() => _ev.Set());

            channel.Say(data);

            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            Assert.IsTrue(wait, "Callback were not being invoked");

            A.CallTo(() => _processorFake.Say(A<Data>._)).WhenArgumentsMatch(collection =>
                {
                    data.ShouldBeEquivalentTo(collection[0]);

                    return true;
                }).MustHaveHappened(Repeated.Like(i => i == 1));
            
            A.CallTo(() => _callbackFake.Say(A<Data>._)).WhenArgumentsMatch(collection =>
                {
                    _replyData.ShouldBeEquivalentTo(collection[0]);

                    return true;
                }).MustHaveHappened(Repeated.Like(i => i == 1));
        }
    }
}