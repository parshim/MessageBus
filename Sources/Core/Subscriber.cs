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

        private readonly ConcurrentDictionary<DataContractKey, DataContract> _registeredTypes = new ConcurrentDictionary<DataContractKey, DataContract>();

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
                        object body;
                        DataContract dataContract;

                        using (XmlDictionaryReader bodyContents = message.GetReaderAtBodyContents())
                        {
                            string name = bodyContents.Name;
                            string ns = bodyContents.NamespaceURI;
                            
                            if (!_registeredTypes.TryGetValue(new DataContractKey(name, ns), out dataContract))
                            {
                                // TODO: Log \ error callback

                                continue;
                            }

                            try
                            {
                                body = dataContract.Serializer.ReadObject(bodyContents);
                            }
                            catch (Exception)
                            {
                                // TODO: Log \ error callback

                                continue;
                            }
                        }
                        
                        Action<object> callback = dataContract.Callback;
                        
                        try
                        {
                            // TODO: Dispatching thread
                            callback(body);
                        }
                        catch (Exception)
                        {
                            // TODO: Log \ error callback

                            continue;
                        }
                    }
                }
            }
        }

        public bool Subscribe<TData>(Action<TData> callback)
        {
            DataContract dataContract = new DataContract(typeof(TData), o => callback((TData)o));

            return _registeredTypes.TryAdd(dataContract.Key, dataContract);
        }

        public bool Subscribe(Type dataType, Action<object> callback)
        {
            DataContract dataContract = new DataContract(dataType, callback);

            return _registeredTypes.TryAdd(dataContract.Key, dataContract);
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