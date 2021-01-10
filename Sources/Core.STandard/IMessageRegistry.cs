using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public interface IMessageRegistry
    {
        bool Register(Type type, MessageFilterInfo filterInfo, ICallHandler handler);
    }
}