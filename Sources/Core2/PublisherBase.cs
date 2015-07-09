using System;
using MessageBus.Core.API;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Core
{
    public abstract class PublisherBase
    {
        protected string _busId;
        
        protected IModel _model;
        private readonly IMessageHelper _messageHelper;

        protected ISendHelper _sendHelper;
        protected PublisherConfigurator _configuration;

        protected PublisherBase(IModel model, string busId, PublisherConfigurator configuration, IMessageHelper messageHelper, ISendHelper sendHelper)
        {
            _model = model;
            _configuration = configuration;
            _messageHelper = messageHelper;
            _sendHelper = sendHelper;
            _busId = busId;

            _model.BasicReturn += ModelOnBasicReturn;
        }

        private void ModelOnBasicReturn(object sender, BasicReturnEventArgs args)
        {
            DataContractKey dataContractKey = args.BasicProperties.GetDataContractKey();

            Type dataType = _sendHelper.GetDataType(dataContractKey);
                
            if (dataType == null)
            {
                dataContractKey = DataContractKey.BinaryBlob;
            }

            object data = _configuration.Serializer.Deserialize(dataContractKey, dataType, args.Body);

            RawBusMessage message = _messageHelper.ConstructMessage(dataContractKey, args.BasicProperties, data);

            OnMessageReturn(args.ReplyCode, args.ReplyText, message);
        }

        protected abstract void OnMessageReturn(int replyCode, string replyText, RawBusMessage message);

        public void Dispose()
        {
            _model.BasicReturn -= ModelOnBasicReturn;

            _model.Close();
        }
    }
}