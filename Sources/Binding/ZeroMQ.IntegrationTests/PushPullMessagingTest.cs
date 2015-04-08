using System;
using System.ServiceModel;
using System.Threading;
using FakeItEasy;
using MessageBus.Binding.ZeroMQ;
using NUnit.Framework;
using ZeroMQ.IntegrationTests.ServiceInterfaces;

namespace ZeroMQ.IntegrationTests
{
    [TestFixture]
    public class PushPullMessagingTest
    {
        private ServiceHost _host;
        private IPublicationService _fake;

        const string Address = "tcp://127.0.0.1:3030";

        [TestFixtureSetUp]
        public void SetUp()
        {
            _fake = A.Fake<IPublicationService>();

            _host = new ServiceHost(new PublicationService(_fake));

            _host.AddServiceEndpoint(typeof(IPublicationService), new ZMQTcpBinding { SocketMode = SocketMode.PushPull }, Address);

            _host.Open();
        }

        [TestFixtureTearDown]
        public void CleanUp()
        {
            _host.Close();
        }

        [Test]
        public void ZMQTcpBinding_Push_Pull()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            A.CallTo(() => _fake.Notify(A<Data>.Ignored)).Invokes(() => ev.Set());

            using (var factory = new ChannelFactory<IPublicationService>(new ZMQTcpBinding { SocketMode = SocketMode.PushPull }, Address))
            {
                IPublicationService channel = factory.CreateChannel();

                channel.Notify(new Data
                    {
                        Name = "Michael",
                        Number = 123
                    });
            }

            bool set = ev.WaitOne(TimeSpan.FromSeconds(10));

            Assert.IsTrue(set);

            A.CallTo(() => _fake.Notify(A<Data>.That.Matches(data => data.Name == "Michael" && data.Number == 123))).MustHaveHappened();
        }
    }
}
