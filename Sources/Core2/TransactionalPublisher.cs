using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class TransactionalPublisher : Publisher, ITransactionalPublisher
    {
        public TransactionalPublisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper)
            : base(model, busId, configuration, messageHelper)
        {
            _model.TxSelect();
        }

        public void Commit()
        {
            _model.TxCommit();
        }

        public void Rallback()
        {
            _model.TxRollback();
        }
    }
}