using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BlobTransferTest
    {
        [Test]
        public void Bus_TransferBlob()
        {
            byte[] blob = new byte[10*1024*1024];
            byte[] received = null;

            int counter = 0;

            ManualResetEvent ev = new ManualResetEvent(false);

            using (IBus bus = new MessageBus.Core.RabbitMQBus(c => c.SetConnectionRetries(50)))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.SetReceiveSelfPublish()))
                {
                    subscriber.Subscribe((byte[] bytes) =>
                        {
                            received = bytes;
                            counter++;
                        });

                    subscriber.Subscribe(new Action<OK>(ok => ev.Set()));

                    subscriber.Open();

                    const int expected = 5;

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        for (int i = 0; i < expected; i++)
                        {
                            publisher.Send(blob);
                        }
                        
                        publisher.Send(new OK());
                    }

                    bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(60));

                    waitOne.Should().BeTrue();

                    counter.Should().Be(expected);
                }
            }
        }
    }
}