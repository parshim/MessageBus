using System;
using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class ConfirmPublisher : Publisher, IConfirmPublisher
    {
        public ConfirmPublisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper)
            : base(model, busId, configuration, messageHelper, sendHelper)
        {
            _model.ConfirmSelect();
        }

        public bool WaitForConfirms(TimeSpan timeOut)
        {
            return _model.WaitForConfirms(timeOut);
        }
    }
}