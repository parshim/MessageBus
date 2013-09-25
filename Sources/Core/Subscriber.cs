using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Subscriber : ISubscriber
    {
        private readonly IInputChannel _inputChannel;
        private readonly Thread _receiver;
        private bool _receive;
        
        private readonly ConcurrentDictionary<Type, IContractHandler> _receivers = new ConcurrentDictionary<Type, IContractHandler>();

        public Subscriber(IInputChannel inputChannel)
        {
            _inputChannel = inputChannel;
            
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
                        MessageBuffer messageBuffer = message.CreateBufferedCopy(50000);

                        foreach (var receiver in _receivers.Values)
                        {
                            IProcessor processor = receiver.CreateProcessor(messageBuffer);

                            if (processor != null)
                            {
                                try
                                {
                                    processor.Process();
                                }
                                catch (Exception)
                                {
                                    // TODO: log
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        public bool Subscribe<TData>(Action<TData> callback)
        {
            return _receivers.TryAdd(typeof(TData), new ContractHandler<TData>(o => callback((TData)o)));
        }

        public bool Subscribe(Type dataType, Action<object> callback)
        {
            Type receiverType = typeof(ContractHandler<>).MakeGenericType(dataType);

            IContractHandler contractHandler = (IContractHandler)Activator.CreateInstance(receiverType, callback);

            return _receivers.TryAdd(dataType, contractHandler);
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
        }
    }
}