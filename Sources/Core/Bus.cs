using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

using MessageBus.Binding.RabbitMQ;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Bus : IBus
    {
        private readonly RabbitMQBinding _binding = new RabbitMQBinding
            {
                ApplicationId = Guid.NewGuid().ToString(),
                IgnoreSelfPublished = true,
                AutoBindExchange = "amq.fanout",
                OneWayOnly = true,
                ExactlyOnce = false,
                PersistentDelivery = false
            };

        private readonly IChannelFactory<IOutputChannel> _channelFactory;
        private readonly IChannelListener<IInputChannel> _listener;
        
        private Thread _receiver;
        private bool _receive;
        private readonly ManualResetEvent _accepting = new ManualResetEvent(false);

        private readonly ConcurrentDictionary<Type, IReceiver> _receivers = new ConcurrentDictionary<Type, IReceiver>();  

        public Bus()
        {
            _channelFactory = _binding.BuildChannelFactory<IOutputChannel>();

            _channelFactory.Open();

            _listener = _binding.BuildChannelListener<IInputChannel>(new Uri("amqp://localhost/"));

            _listener.Open();

            _listener.BeginAcceptChannel(ChennelAccepted, null);
        }

        public WaitHandle AcceptHandle
        {
            get
            {
                return _accepting;
            }
        }

        private void ChennelAccepted(IAsyncResult ar)
        {
            IInputChannel channel = _listener.EndAcceptChannel(ar);

            if (channel == null)
            {
                _listener.BeginAcceptChannel(ChennelAccepted, null);

                return;
            }

            channel.Faulted += ChannelOnFaulted;

            _receive = true;

            _receiver = new Thread(Start);
            _receiver.Start(channel);
        }

        private void ChannelOnFaulted(object sender, EventArgs eventArgs)
        {
            IInputChannel channel = (IInputChannel)sender;

            channel.Faulted -= ChannelOnFaulted;

            _receive = false;

            _listener.BeginAcceptChannel(ChennelAccepted, null);
        }

        private void Start(object o)
        {
            IInputChannel channel = (IInputChannel) o;

            channel.Open();

            _accepting.Set();
            
            try
            {
                while (_receive)
                {
                    Message message;

                    if (channel.TryReceive(TimeSpan.FromMilliseconds(100), out message))
                    {
                        using (message)
                        {
                            MessageBuffer messageBuffer = message.CreateBufferedCopy((int) _binding.MaxMessageSize);

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
            finally
            {
                channel.Close();

                _accepting.Reset();
            }
        }

        public void Dispose()
        {
            _receive = false;

            _channelFactory.Close();
            _listener.Close();
        }

        public IPublisher CreatePublisher()
        {
            IOutputChannel outputChannel = _channelFactory.CreateChannel(new EndpointAddress("amqp://localhost/amq.fanout"));

            return new Publisher(outputChannel, _binding.MessageVersion);
        }

        public bool Register<TData>(Action<TData> callback)
        {
            return _receivers.TryAdd(typeof (TData), new Receiver<TData>(o => callback((TData)o)));
        }
        
        public bool Register(Type dataType, Action<object> callback)
        {
            Type receiverType = typeof (Receiver<>).MakeGenericType(dataType);

            IReceiver receiver = (IReceiver)Activator.CreateInstance(receiverType, callback);

            return _receivers.TryAdd(dataType, receiver);
        }

        public bool RegisterHierarchy<TData>(Action<TData> callback)
        {
            Type baseType = typeof (TData);

            var types = from type in baseType.Assembly.GetTypes()
                        where type != baseType && baseType.IsAssignableFrom(type)
                        select type;

            bool atLeastOne = false;

            foreach (Type type in types)
            {
                atLeastOne = Register(type, o => callback((TData)o)) || atLeastOne;
            }

            return atLeastOne;
        }

    }
}
