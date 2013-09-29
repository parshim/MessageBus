using System;
using System.Runtime.Serialization;
using System.Xml;

using MessageBus.Core.API;

namespace MessageBus.Core
{
    class Dispatcher : IDispatcher
    {
        private readonly DataContractSerializer _serializer;
        private readonly Action<object> _visitor;

        public Dispatcher(DataContractSerializer serializer, Action<object> visitor)
        {
            _serializer = serializer;
            _visitor = visitor;
        }
        
        public void Dispatch(XmlDictionaryReader bodyContents)
        {
            object body;

            try
            {
                body = _serializer.ReadObject(bodyContents);
            }
            catch (Exception)
            {
                // TODO: Log \ error callback

                return;
            }

            try
            {
                // TODO: Dispatching thread
                _visitor(body);
            }
            catch (Exception)
            {
                // TODO: Log \ error callback

                return;
            }
        }
    }
}
