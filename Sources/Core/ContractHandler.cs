using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class ContractHandler<T> : IContractHandler
    {
        private readonly Action<object> _callback;
        private readonly DataContractSerializer _serializer;
        
        public ContractHandler(Action<object> callback)
        {
            _callback = callback;
            _serializer = new DataContractSerializer(typeof(T));
        }

        public string Name
        {
            get { return null; }
        }

        public string NameSpace
        {
            get
            {
                 return null;
            }
        }

        public IProcessor CreateProcessor(MessageBuffer messageBuffer)
        {
            using (Message message = messageBuffer.CreateMessage())
            {
                T body;

                try
                {
                    body = message.GetBody<T>(_serializer);
                }
                catch (Exception)
                {
                    return null;
                }

                return new Processor<object>(_callback, body);
            }
        }
    }
}