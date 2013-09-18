using System;
using System.ServiceModel;
using System.Threading;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.IntegrationTests.ContractsAndServices;

namespace RabbitMQ.IntegrationTests
{
    public abstract class DuplexDeliveryTestBase
    {
        protected ServiceHost _host;
        protected ChannelFactory<IDuplexService> _channelFactory;
        protected readonly ManualResetEventSlim _ev = new ManualResetEventSlim();

        protected IDuplexService _processorFake = A.Fake<IDuplexService>();
        protected IDuplexService _errorProcessorFake = A.Fake<IDuplexService>();
        protected IDuplexService _callbackFake = A.Fake<IDuplexService>();

        [TestCleanup]
        public void TestCleanup()
        {
            _host.Close(TimeSpan.FromSeconds(2));

            _channelFactory.Close(TimeSpan.FromSeconds(2));

            _ev.Dispose();
        }
    }
}