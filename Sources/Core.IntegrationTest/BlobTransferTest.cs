using System;
using System.Threading;
using System.Xml;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BlobTransferTest
    {
        [TestMethod]
        public void Bus_TransferBlob()
        {
            byte[] blob = new byte[10*1024*1024];
            byte[] received = null;

            ManualResetEvent ev = new ManualResetEvent(false);

            using (IBus bus = new RabbitMQBus(readerQuotas: new XmlDictionaryReaderQuotas { MaxArrayLength = blob.Length * 2}))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((byte[] bytes) =>
                        {
                            received = bytes;
                            ev.Set();
                        });

                    subscriber.StartProcessMessages();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(blob);
                    }

                    bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                    waitOne.Should().BeTrue();

                    received.Should().BeEquivalentTo(blob);
                }
            }
        }
    }
}