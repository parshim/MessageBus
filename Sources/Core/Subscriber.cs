using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Subscriber : ISubscriber
    {
        private readonly IChannelListener<IInputChannel> _listener;
        private readonly IInputChannel _inputChannel;
        private readonly Thread _receiver;
        private bool _receive;

        private readonly ConcurrentDictionary<DataContractKey, IDispatcher> _registeredTypes = new ConcurrentDictionary<DataContractKey, IDispatcher>();

        public Subscriber(IChannelListener<IInputChannel> listener)
        {
            _listener = listener;

            _listener.Open();

            _inputChannel = _listener.AcceptChannel();
            
            _inputChannel.Open();

            _receive = true;

            _receiver = new Thread(ProcessMessages);
            _receiver.Start();
        }

        private void ProcessMessages()
        {
            while (_receive)
            {
                Message message;

                if (_inputChannel.TryReceive(TimeSpan.FromMilliseconds(100), out message))
                {
                    using (message)
                    {
                        using (XmlDictionaryReader bodyContents = message.GetReaderAtBodyContents())
                        {
                            string name = bodyContents.Name;
                            string ns = bodyContents.NamespaceURI;

                            IDispatcher dispatcher;
                            if (!_registeredTypes.TryGetValue(new DataContractKey(name, ns), out dispatcher))
                            {
                                // TODO: Log \ error callback

                                continue;
                            }

                            dispatcher.Dispatch(bodyContents);
                        }
                    }
                }
            }
        }

        public bool Subscribe<TData>(Action<TData> callback)
        {
            return Subscribe(typeof (TData), o => callback((TData) o));
        }

        public bool Subscribe(Type dataType, Action<object> callback)
        {
            DataContract dataContract = new DataContract(dataType);

            return _registeredTypes.TryAdd(dataContract.Key, new Dispatcher(dataContract.Serializer, callback));
        }

        public bool SubscribeHierarchy<TData>(Action<TData> callback)
        {
            Type baseType = typeof(TData);

            var types = from type in baseType.Assembly.GetTypes()
                        where type != baseType && baseType.IsAssignableFrom(type)
                        select type;

            bool atLeastOne = false;

            foreach (Type type in types)
            {
                atLeastOne = Subscribe(type, o => callback((TData)o)) || atLeastOne;
            }

            return atLeastOne;
        }

        public void Dispose()
        {
            _receive = false;

            _receiver.Join(TimeSpan.FromMilliseconds(200));

            _inputChannel.Close();

            _listener.Close();
        }
    }
}