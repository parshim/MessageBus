using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using MessageBus.Core.Proxy;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Core.Proxy.IntegrationTests
{
    [TestFixture]
    public class FullLoopTest
    {
        [Test]
        public void Send_Receive_Message()
        {
            using (IBus bus = new RabbitMQBus())
            {
                using (IChannelFactory<ITestContract> channelFactory = new MessageBus.Core.Proxy.ChannelFactory<ITestContract>(bus))
                {
                    ISubscriptionFactory<ITestContract> subscriptionFactory = new SubscriptionFactory<ITestContract>(bus);

                    Data actual = null;
                    ManualResetEvent ev = new ManualResetEvent(false);

                    Data data = new Data
                    {
                        Value = "boo"
                    };

                    using (ISubscriptionSelector<ITestContract> selector = subscriptionFactory.Subscribe(c => c.SetReceiveSelfPublish()))
                    {
                        selector.Subscribe<Data>(contract => contract.Foo, s =>
                        {
                            actual = s;

                            ev.Set();
                        });


                        ITestContract channel = channelFactory.CreateChannel();

                        channel.Foo(data);

                        ev.WaitOne(TimeSpan.FromSeconds(5));

                        actual.ShouldBeEquivalentTo(data);
                    }
                }
            }
        }
        
        [Test]
        public void Send_Receive_CustomSerializationMessage()
        {
            using (IBus bus = new RabbitMQBus())
            {
                using (IChannelFactory<ITestContract> channelFactory = new MessageBus.Core.Proxy.ChannelFactory<ITestContract>(bus, c => c.UseJsonSerializerSettings(new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                })))
                {
                    ISubscriptionFactory<ITestContract> subscriptionFactory = new SubscriptionFactory<ITestContract>(bus);

                    Data actual = null;
                    ManualResetEvent ev = new ManualResetEvent(false);

                    Data data = new Data
                    {
                        Value = "boo"
                    };

                    using (ISubscriptionSelector<ITestContract> selector = subscriptionFactory.Subscribe(c => c.SetReceiveSelfPublish().UseJsonSerializerSettings(new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    })))
                    {
                        selector.Subscribe<Data>(contract => contract.Foo, s =>
                        {
                            actual = s;

                            ev.Set();
                        });


                        ITestContract channel = channelFactory.CreateChannel();

                        channel.Foo(data);

                        ev.WaitOne(TimeSpan.FromSeconds(5));

                        actual.ShouldBeEquivalentTo(data);
                    }
                }
            }
        }
    }

    [ServiceContract(Namespace = "http://www.test.org")]
    public interface ITestContract
    {
        [OperationContract]
        void Foo(Data data);
    }

    [DataContract(Namespace = "http://www.data.org")]
    public class Data
    {
        [DataMember]
        public string Value { get; set; }
    }
}
