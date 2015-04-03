using System;
using System.ServiceModel;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using MessageBus.Core.Proxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Proxy.IntegrationTests
{
    [TestClass]
    public class FullLoopTest
    {
        [TestMethod]
        public void Send_Receive_Message()
        {
            using (IBus bus = new RabbitMQBus())
            {
                using (IChannelFactory<ITestContract> channelFactory = new MessageBus.Core.Proxy.ChannelFactory<ITestContract>(bus))
                {
                    ISubscriptionFactory<ITestContract> subscriptionFactory = new SubscriptionFactory<ITestContract>(bus, c => c.SetReceiveSelfPublish());

                    string actual = "";
                    ManualResetEvent ev = new ManualResetEvent(false);

                    using (ISubscriptionSelector<ITestContract> selector = subscriptionFactory.Subscribe())
                    {
                        selector.Subscribe<string>(contract => contract.Foo, s =>
                        {
                            actual = s;

                            ev.Set();
                        });


                        ITestContract channel = channelFactory.CreateChannel();

                        channel.Foo("boo");

                        ev.WaitOne(TimeSpan.FromSeconds(5));

                        actual.Should().Be("boo");
                    }
                }
            }
        }
    }

    [ServiceContract]
    public interface ITestContract
    {
        [OperationContract]
        void Foo(string vale);
    }
}
