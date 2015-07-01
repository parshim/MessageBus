using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class ConfirmPublisher : Publisher, IConfirmPublisher
    {
        public ConfirmPublisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper)
            : base(model, busId, configuration, messageHelper)
        {
            _model.ConfirmSelect();
        }

        public bool WaitForConfirms(TimeSpan timeOut)
        {
            return _model.WaitForConfirms(timeOut);
        }
    }
}