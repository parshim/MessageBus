using System;

namespace MessageBus.Core.API
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SubscribtionAttribute : Attribute
    {
    }
}
