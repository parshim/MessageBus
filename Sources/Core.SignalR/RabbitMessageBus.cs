using System;
using System.Collections.Generic;
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

        public RabbitMessageBus(IDependencyResolver resolver, RabbitScaleoutConfiguration configuration)
            : base(resolver, configuration)
        {
            _bus = new RabbitMQBus(new RabbitMQConnectionString(new Uri(configuration.ConnectionString)));

            ScaleoutMessageSerializer serializer = new ScaleoutMessageSerializer();

            _publisher = _bus.CreatePublisher(c => c.UseCustomSerializer(serializer));
            _subscriber = _bus.CreateSubscriber(c => c.SetReceiveSelfPublish().AddCustomSerializer(serializer));

            _subscriber.Subscribe<ScaleoutMessage>(OnMessage, filter: configuration.FilterHeaders);

            Open(0);

            _subscriber.Open();
        }

        protected override Task Send(int streamIndex, IList<Message> messages)
        {
            return Task.Factory.StartNew(() => _publisher.Send(new ScaleoutMessage(messages)));
        }

        private void OnMessage(ScaleoutMessage message)
        {
            OnReceived(0, 0, message);
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
