using System;
using System.ServiceModel;
using System.Threading;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.IntegrationTests.ContractsAndServices;

namespace RabbitMQ.IntegrationTests
{
    public abstract class OneWayDeliveryTestBase
    {
        protected ServiceHost _host;
        protected ChannelFactory<IOneWayService> _channelFactory;
        protected readonly ManualResetEventSlim _ev = new ManualResetEventSlim();

        protected IOneWayService _processorFake = A.Fake<IOneWayService>();
        protected IOneWayService _errorProcessorFake = A.Fake<IOneWayService>();

        [TestCleanup]
        public virtual void TestCleanup()
        {
            _host.Close(TimeSpan.FromSeconds(2));

            _channelFactory.Close(TimeSpan.FromSeconds(2));

            _ev.Dispose();
        }
    }
}