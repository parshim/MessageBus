using MessageBus.Core.API;
using RabbitMQ.Client;

namespace MessageBus.Core
{
    public class TransactionalPublisher : Publisher, ITransactionalPublisher
    {
        public TransactionalPublisher(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper)
            : base(model, busId, configuration, messageHelper, sendHelper)
        {
            _model.TxSelect();
        }

        public void Commit()
        {
            _model.TxCommit();
        }

        public void Rollback()
        {
            _model.TxRollback();
        }
    }
}