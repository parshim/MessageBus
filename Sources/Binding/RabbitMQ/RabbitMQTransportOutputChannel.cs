using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Transactions;
using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    internal sealed class RabbitMQTransportOutputChannel : RabbitMQOutputChannelBase
    {
        private readonly RabbitMQTransportBindingElement _bindingElement;
        private readonly MessageEncoder _encoder;
        private IModel _model;
        private bool _transactional;

        private readonly List<string> _transactionIdentifiers = new List<string>();

        public RabbitMQTransportOutputChannel(BindingContext context, EndpointAddress address, Uri via)
            : base(context, address, via)
        {

            _bindingElement = context.Binding.Elements.Find<RabbitMQTransportBindingElement>();

            MessageEncodingBindingElement encoderElement;

            if (_bindingElement.MessageFormat == MessageFormat.MTOM)
            {
                encoderElement = context.Binding.Elements.Find<MtomMessageEncodingBindingElement>();
            }
            else
            {
                encoderElement = context.Binding.Elements.Find<TextMessageEncodingBindingElement>();
            }

            if (encoderElement != null) 
            {
                _encoder = encoderElement.CreateMessageEncoderFactory().Encoder;
            }
        }

        public override void Open(TimeSpan timeout)
        {
            if (State != CommunicationState.Created && State != CommunicationState.Closed)
                throw new InvalidOperationException(string.Format("Cannot open the channel from the {0} state.", State));
            
            OnOpening();
#if VERBOSE
            DebugHelper.Start();
#endif
            _model = ConnectionManager.Instance.OpenModel(new RabbitMQUri(RemoteAddress.Uri), _bindingElement.BrokerProtocol, timeout);

            if (Transaction.Current != null)
            {
                _model.TxSelect();

                _transactional = true;
            }

#if VERBOSE
            DebugHelper.Stop(" ## Out.Open {{Time={0}ms}}.");
#endif
            OnOpened();
        }
        
        public override void Close(TimeSpan timeout)
        {
            if (State == CommunicationState.Closed || State == CommunicationState.Closing)
                return; // Ignore the call, we're already closing.

            OnClosing();

#if VERBOSE
            DebugHelper.Start();
#endif

            if (_model != null)
            {
                ConnectionManager.Instance.CloseModel(_model, timeout);
                _model = null;
            }

#if VERBOSE
            DebugHelper.Stop(" ## Out.Close {{Time={0}ms}}.");
#endif
            OnClosed();
        }

        public override void Send(Message message, TimeSpan timeout)
        {
            if (State == CommunicationState.Opened && !message.IsFault)
            {
#if VERBOSE
                DebugHelper.Start();
#endif
                byte[] body;

                IBasicProperties basicProperties = _model.CreateBasicProperties();

                // Set message properties
                SetMessageProperties(basicProperties);

                // Read custom headers and put it into the message headers
                SetMessageHeaders(message, basicProperties);

                // Set ReplyTo address if specified
                if (_bindingElement.ReplyToExchange != null)
                {
                    message.Headers.ReplyTo = new EndpointAddress(_bindingElement.ReplyToExchange);
                }

                // Serialize the message to stream
                using (MemoryStream str = new MemoryStream())
                {
                    _encoder.WriteMessage(message, str);
                    body = str.ToArray();
                }

                // Build AMQP uri
                RabbitMQUri uri = new RabbitMQUri(RemoteAddress.Uri);

                // Enlist operation into transaction 
                EnlistTransaction();

                // Publish AMQP message
                _model.BasicPublish(uri.Endpoint,
                                     uri.RoutingKey,
                                     basicProperties,
                                     body);

#if VERBOSE
                DebugHelper.Stop(" #### Message.Send {{\n\tAction={2}, \n\tBytes={1}, \n\tTime={0}ms}}.",
                    body.Length,
                    message.Headers.Action.Remove(0, message.Headers.Action.LastIndexOf('/')));
#endif
            }
        }

        private void SetMessageProperties(IBasicProperties basicProperties)
        {
            basicProperties.Timestamp = new AmqpTimestamp(GetUnixTime(DateTime.Now));
            basicProperties.ContentType = _encoder.ContentType;
            basicProperties.DeliveryMode = _bindingElement.PersistentDelivery ? (byte)2 : (byte)1;
            basicProperties.AppId = _bindingElement.ApplicationId ?? "";
            basicProperties.Headers = new Hashtable();

            if (!string.IsNullOrEmpty(_bindingElement.TTL))
            {
                basicProperties.Expiration = _bindingElement.TTL;
            }
        }

        private void SetMessageHeaders(Message message, IBasicProperties basicProperties)
        {
            string nsToMap = _bindingElement.HeaderNamespace;

            if (string.IsNullOrEmpty(nsToMap)) return;

            foreach (MessageHeaderInfo messageHeaderInfo in message.Headers.Where(info => info.Namespace == nsToMap))
            {
                // TODO: add support for bool and int types
                string header = message.Headers.GetHeader<string>(messageHeaderInfo.Name,
                                                                  messageHeaderInfo.Namespace,
                                                                  messageHeaderInfo.Actor);

                basicProperties.Headers.Add(messageHeaderInfo.Name, header);
            }
        }

        private void EnlistTransaction()
        {
            if (_transactional)
            {
                Transaction current = Transaction.Current;

                if (current == null)
                {
                    throw new FaultException(
                        "Channel used inside transaction scope can not be used without transaction thereafter. Reopen client channel.");
                }

                if (!_transactionIdentifiers.Contains(current.TransactionInformation.LocalIdentifier))
                {
                    current.EnlistVolatile(new TransactionalDispatchingEnslistment(_model), EnlistmentOptions.None);

                    current.TransactionCompleted += CurrentOnTransactionCompleted;

                    _transactionIdentifiers.Add(current.TransactionInformation.LocalIdentifier);
                }
            }
        }

        private void CurrentOnTransactionCompleted(object sender, TransactionEventArgs transactionEventArgs)
        {
            transactionEventArgs.Transaction.TransactionCompleted -= CurrentOnTransactionCompleted;

            _transactionIdentifiers.Remove(transactionEventArgs.Transaction.TransactionInformation.LocalIdentifier);
        }

        private long GetUnixTime(DateTime dateTime)
        {
            return Convert.ToInt64((dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds);
        }
    }

    internal class TransactionalDispatchingEnslistment : IEnlistmentNotification
    {
        private readonly IModel _model;

        public TransactionalDispatchingEnslistment(IModel model)
        {
            _model = model;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            if (_model.IsOpen)
            {
                _model.TxCommit();
            }

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            if (_model.IsOpen)
            {
                _model.TxRollback();
            }

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}
