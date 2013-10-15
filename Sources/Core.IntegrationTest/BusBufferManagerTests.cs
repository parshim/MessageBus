using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusBufferManagerTests
    {
        [DataContract]
        public class Blob
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public byte[] Data { get; set; }
        }

        [TestMethod]
        public void ZeroMQBus_BufferManager_ExhangeBinaryBlobs()
        {
            using (ZeroMQBus entityA = new ZeroMQBus("127.0.0.1", 2525, readerQuotas: new XmlDictionaryReaderQuotas
                {
                    MaxArrayLength = 10 * 1024 * 1024
                }), 
                             entityB = new ZeroMQBus("127.0.0.1", 2525))
            {
                Blob data = new Blob
                {
                    Id = 1,
                    Data = new byte[8 * 1024 * 1024] // 8MB
                };

                int received = 0;

                ManualResetEvent ev1 = new ManualResetEvent(false);

                using (ISubscriber subscriberA = entityA.CreateSubscriber(BufferManager.CreateBufferManager(3, 10 * 1024 * 1024)))
                {
                    subscriberA.Subscribe(delegate(Blob blob) { received++; ev1.Set(); });

                    subscriberA.StartProcessMessages();

                    using (IPublisher publisher = entityB.CreatePublisher(BufferManager.CreateBufferManager(3, 10 * 1024 * 1024)))
                    {
                        publisher.Send(data);
                    }

                    bool waitOne = ev1.WaitOne(TimeSpan.FromSeconds(10));

                    waitOne.Should().BeTrue("Message not received");

                    received.Should().Be(1);
                }
            }
        }
    }
}
