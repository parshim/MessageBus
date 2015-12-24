using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class NullCallHandler : ICallHandler
    {
        public Task<RawBusMessage> Dispatch(RawBusMessage message)
        {
            return Task.FromResult(new RawBusMessage());
        }
    }
}