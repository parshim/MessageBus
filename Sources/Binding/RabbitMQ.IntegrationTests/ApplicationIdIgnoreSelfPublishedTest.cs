using System;
using System.ServiceModel;
using FakeItEasy;
using FluentAssertions;
using MessageBus.Binding.RabbitMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.IntegrationTests.ContractsAndServices;

namespace RabbitMQ.IntegrationTests
{
    [TestClass]
    public class ApplicationIdIgnoreSelfPublishedTest : OneWayDeliveryTestBase
    {
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
        [TestInitialize]
        public void TestInitialize()
        {
            _host = new ServiceHost(new OneWayService(_processorFake, _errorProcessorFake));

            const string serviceAddress = "amqp://localhost/myQueue?routingKey=OneWayService";

            _host.AddServiceEndpoint(typeof(IOneWayService), new RabbitMQBinding
            {
                AutoBindExchange = "amq.direct", // If not null, queue will be automatically binded to the exchange using provided routingKey (if any)
                ExactlyOnce = false, // Non-transactional consumption,
                OneWayOnly = true, // Use False only if calback communication required
                //TTL = 1000, // Message time to leave in miliseconds
                //PersistentDelivery = true // If true, every message will be written to disk on rabbitMQ broker side before dispatching to the destination(s)
                ApplicationId = "MyApp",
                IgnoreSelfPublished = true
            }, serviceAddress);

            _host.Open();


            const string clientAddress = "amqp://localhost/amq.direct?routingKey=OneWayService";

            _channelFactory = new ChannelFactory<IOneWayService>(new RabbitMQBinding
            {
                OneWayOnly = true,
                ApplicationId = "MyApp"
            }, clientAddress);

            _channelFactory.Open();
        }

        [TestMethod]
        public void Test_MessageWithSameAppId_DropedWithIgnoreSet()
        {
            IOneWayService channel = _channelFactory.CreateChannel();

            Data data = new Data
            {
                Id = 1,
                Name = "Rabbit"
            };

            A.CallTo(_errorProcessorFake).DoesNothing();
            A.CallTo(() => _processorFake.Say(A<Data>.Ignored)).Invokes(() => _ev.Set());

            channel.Say(data);

            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            wait.Should().BeFalse("message with same application id should be dropped,IgnoreSelfPublished = true");
        }
    }
}
