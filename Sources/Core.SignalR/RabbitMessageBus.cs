using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

using ISubscriber = MessageBus.Core.API.ISubscriber;

namespace Core.SignalR
{
    public class RabbitMessageBus : ScaleoutMessageBus
    {
        private readonly IBus _bus;

        private readonly IPublisher _publisher;
        private readonly ISubscriber _subscriber;
        
        private const string StreamIndexHeader = "streamIndex";

        private static long _payloadId;

        public RabbitMessageBus(IDependencyResolver resolver, RabbitScaleoutConfiguration configuration)
            : base(resolver, configuration)
        {
            _bus = new RabbitMQBus(c => c.UseConnectionString(configuration.ConnectionString));

            ScaleoutMessageSerializer serializer = new ScaleoutMessageSerializer();

            _publisher = _bus.CreatePublisher(c => c.UseCustomSerializer(serializer));
            _subscriber = _bus.CreateSubscriber(c => c.SetReceiveSelfPublish().AddCustomSerializer(serializer));

            _subscriber.Subscribe<ScaleoutMessage>(OnMessage, filter: configuration.FilterHeaders);

            Open(0);

            _subscriber.Open();
        }

        protected override Task Send(int streamIndex, IList<Message> messages)
        {
            return Task.Factory.StartNew(() =>
            {
                var busMessage = new BusMessage<ScaleoutMessage>
                {
                    Data = new ScaleoutMessage(messages)
                };

                busMessage.Headers.Add(new BusHeader
                {
                    Name = StreamIndexHeader,
                    Value = streamIndex.ToString()
                });

                _publisher.Send(busMessage);
            });
        }

        private void OnMessage(BusMessage<ScaleoutMessage> message)
        {
            string sIndex = message.Headers.Where(h => h.Name == StreamIndexHeader).Select(h => h.Value).FirstOrDefault();

            int streamIndex;

            Int32.TryParse(sIndex, out streamIndex);

            OnReceived(streamIndex, (ulong)Interlocked.Increment(ref _payloadId), message.Data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _publisher.Dispose();
                _subscriber.Dispose();

                _bus.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
