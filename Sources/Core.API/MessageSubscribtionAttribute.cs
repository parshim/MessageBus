using System;

namespace MessageBus.Core.API
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MessageSubscribtionAttribute : Attribute
    {
        /// <summary>
        /// Looking for derived types and automaticaly register them for the same callback
        /// </summary>
        public bool RegisterHierarchy { get; set; }
        
        /// <summary>
        /// If true, messages of this type published whithin bus instance will be received and processed by subscriber. Otherwise ignored.
        /// </summary>
        public bool ReceiveSelfPublish { get; set; }
    }
}