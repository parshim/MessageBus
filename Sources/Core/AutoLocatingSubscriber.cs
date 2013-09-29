using System.ServiceModel.Channels;
using MessageBus.Core.API;
using Microsoft.Practices.ServiceLocation;

namespace MessageBus.Core
{
    public class AutoLocatingSubscriber : Subscriber, IAutoLocatingSubscriber
    {
        private readonly IServiceLocator _serviceLocator;

        public AutoLocatingSubscriber(IServiceLocator serviceLocator, IChannelListener<IInputChannel> listener) : base(listener)
        {
            _serviceLocator = serviceLocator;
        }

        public bool Subscribe<TData>()
        {
            return Subscribe<TData>(data =>
                {
                    IProcessor<TData> processor = _serviceLocator.GetInstance<IProcessor<TData>>();

                    processor.Process(data);
                });
        }
    }
}