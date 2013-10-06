using System;
using System.ServiceModel;
using System.Xml;
using FakeItEasy;
using FluentAssertions;

using MessageBus.Binding.RabbitMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.IntegrationTests.ContractsAndServices;

namespace RabbitMQ.IntegrationTests
{
    [TestClass]
    public class MessageFormatsTest : OneWayDeliveryTestBase
    {
        private ChannelFactory<IOneWayService> _mtomFactory;

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

            const string serviceAddress = "amqp://localhost//?routingKey=testFormats";

            _host.AddServiceEndpoint(typeof(IOneWayService), new RabbitMQBinding
                {
                    AutoBindExchange = "amq.direct", // If not null, queue will be automatically binded to the exchange using provided routingKey (if any)
                    ExactlyOnce = false, // Non-transactional consumption,
                    OneWayOnly = true, // Use False only if calback communication required
                    //TTL = 1000, // Message time to leave in miliseconds
                    //PersistentDelivery = true // If true, every message will be written to disk on rabbitMQ broker side before dispatching to the destination(s)
                    ReaderQuotas = new XmlDictionaryReaderQuotas
                        {
                            MaxArrayLength = 2 * 1024 * 1024
                        }
                }, serviceAddress);

            _host.Open();


            const string clientAddress = "amqp://localhost/amq.direct?routingKey=testFormats";

            _channelFactory = new ChannelFactory<IOneWayService>(new RabbitMQBinding
                {
                    OneWayOnly = true,
                    MessageFormat = MessageFormat.Text
                }, clientAddress);

            _mtomFactory = new ChannelFactory<IOneWayService>(new RabbitMQBinding
                {
                    OneWayOnly = true,
                    MessageFormat = MessageFormat.MTOM
                }, clientAddress);

            _channelFactory.Open();
            _mtomFactory.Open();
        }

        public override void TestCleanup()
        {
            base.TestCleanup();

            _mtomFactory.Close();
        }


        [TestMethod]
        public void RabbitMQBinding_DeliveryMessageInAllSupportedFromats_ReceiveInSingleChannel()
        {
            IOneWayService textChannel = _channelFactory.CreateChannel();
            IOneWayService mtomChannel = _mtomFactory.CreateChannel();

            Blob data = new Blob
                {
                    Id = 1,
                    Data = new byte[1024]
                };

            int receiveCounter = 0;

            A.CallTo(_errorProcessorFake).DoesNothing();
            A.CallTo(() => _processorFake.LargeData(A<Blob>.Ignored)).Invokes(() => receiveCounter++);
            A.CallTo(() => _processorFake.Say(A<Data>.Ignored)).Invokes(() => _ev.Set());

            textChannel.LargeData(data);
            mtomChannel.LargeData(data);

            textChannel.Say(new Data());

            bool wait = _ev.Wait(TimeSpan.FromSeconds(10));

            receiveCounter.Should().Be(2);

            wait.Should().BeTrue("Service were not being invoked");
        }
    }
}
