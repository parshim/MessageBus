using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Receiver<T> : IReceiver
    {
        private readonly Action<object> _callback;
        private readonly DataContractSerializer _serializer;
        
        public Receiver(Action<object> callback)
        {
            _callback = callback;
            _serializer = new DataContractSerializer(typeof(T));
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