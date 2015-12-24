using System.Threading.Tasks;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public interface ICallHandler
    {
        Task<RawBusMessage> Dispatch(RawBusMessage message);
    }

}