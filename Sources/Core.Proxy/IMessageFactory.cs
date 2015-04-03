using System;
using System.Reflection;

namespace MessageBus.Core.Proxy
{
    public interface IMessageFactory
    {
        object CreateMessage(MethodInfo methodInfo, object[] values);

        Type GetMessageType(MethodInfo methodInfo);

        FieldInfo GetMessageFieldInfo(MethodInfo methodInfo, string fieldName);
    }
}
