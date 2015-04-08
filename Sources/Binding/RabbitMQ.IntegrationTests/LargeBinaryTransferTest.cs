using System;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using MessageBus.Binding.RabbitMQ;
using NUnit.Framework;
using RabbitMQ.IntegrationTests.ContractsAndServices;

namespace RabbitMQ.IntegrationTests
{
    [TestFixture]
    public class LargeBinaryTransferTest
    {
        protected ServiceHost _host;
        protected ChannelFactory<IOneWayService> _channelFactory;
        protected readonly ManualResetEventSlim _ev = new ManualResetEventSlim();

        protected IOneWayService _processorFake = A.Fake<IOneWayService>();
        protected IOneWayService _errorProcessorFake = A.Fake<IOneWayService>();

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
        ///   |           |        node hostname |       |            |         Specifies routing key value, may be empty
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
            _host = new ServiceHost(new OneWayService(_processorFake, _errorProcessorFake));

            const string serviceAddress = "amqp://localhost/largeQueue?routingKey=large";

            _host.AddServiceEndpoint(typeof(IOneWayService), new RabbitMQBinding
            {
                AutoBindExchange = "amq.direct", // If not null, queue will be automatically binded to the exchange using provided routingKey (if any)
                ExactlyOnce = false, // Non-transactional consumption,
                OneWayOnly = true, // Use False only if calback communication required
                //TTL = 1000, // Message time to leave in miliseconds
                //PersistentDelivery = true // If true, every message will be written to disk on rabbitMQ broker side before dispatching to the destination(s)
                ApplicationId = "MyApp",
                ReaderQuotas = new XmlDictionaryReaderQuotas
                    {
                        MaxArrayLength = 10 * 1024 * 1024 // 10MB
                    }
            }, serviceAddress);

            _host.Open();


            const string clientAddress = "amqp://localhost/amq.direct?routingKey=large";

            _channelFactory = new ChannelFactory<IOneWayService>(new RabbitMQBinding
            {
                OneWayOnly = true,
                ApplicationId = "MyApp"
            }, clientAddress);

            _channelFactory.Open();
        }

        [Test]
        public void RabbitMQBinding_TransferLargeBinary_TextBaseSerialization()
        {
            IOneWayService channel = _channelFactory.CreateChannel();

            Blob data = new Blob
            {
                Id = 1,
                Data = new byte[8 * 1024 * 1024] // 8MB
            };
            
            A.CallTo(_errorProcessorFake).DoesNothing();
            A.CallTo(() => _processorFake.LargeData(A<Blob>.Ignored)).Invokes(() => _ev.Set());

            channel.LargeData(data);

            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            wait.Should().BeTrue();

            A.CallTo(() => _processorFake.LargeData(A<Blob>._)).MustHaveHappened(Repeated.Like(i => i == 1));
        }
    }
}
