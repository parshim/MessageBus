using System;

namespace MessageBus.Core.API
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MessageSubscriptionAttribute : Attribute
    {
        /// <summary>
        /// Looking for derived types and automatically register them for the same callback
        /// </summary>
        public bool RegisterHierarchy { get; set; }
    }
}