using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class AsyncSubscriberTest : IErrorSubscriber
    {
        private readonly ManualResetEvent _ev = new ManualResetEvent(false);

        [Test]
        public void Bus_AsyncSubscriberException_ErrorHandlerCalled()
        {
            ImportiantData data = new ImportiantData { Info = "Valuable information" };


            // Second bus subscribes to message after it was dispatched and should receive it
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (IAsyncSubscriber subscriber = bus.CreateAsyncSubscriber(c => c.SetReceiveSelfPublish().UseErrorSubscriber(this)))
                {
                    subscriber.Subscribe<ImportiantData>(Subscribe);

                    subscriber.Open();
                    
                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        publisher.Send(data);
                    }

                    bool wait = _ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue();
                }
            }
        }

        private async Task Subscribe(ImportiantData data)
        {
            await Task.Run(() =>
            {
                throw new Exception();
            });
        }

        public void MessageDeserializeException(RawBusMessage busMessage, Exception exception)
        {
            
        }

        public void MessageDispatchException(RawBusMessage busMessage, Exception exception)
        {
            _ev.Set();
        }

        public void MessageFilteredOut(RawBusMessage busMessage)
        {
        }

        public void UnregisteredMessageArrived(RawBusMessage busMessage)
        {
        }
    }
}
