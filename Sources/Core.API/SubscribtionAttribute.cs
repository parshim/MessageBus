using System;
using System.Collections.Generic;

namespace MessageBus.Core.API
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SubscribtionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageSubscribtionAttribute : Attribute
    {
        private readonly List<BusHeader> _filters = new List<BusHeader>();
        
        public MessageSubscribtionAttribute(params BusHeader[] filters)
        {
            _filters.AddRange(filters);
        }

        /// <summary>
        /// Looking for derived types and automaticaly register them for the same callback
        /// </summary>
        public bool RegisterHierarchy { get; set; }
        
        /// <summary>
        /// If true, messages of this type published whithin bus instance will be received and processed by subscriber. Otherwise ignored.
        /// </summary>
        public bool ReceiveSelfPublish { get; set; }

        /// <summary>
        /// Subscribe to message which sent only with specified headers.
        /// </summary>
        public IEnumerable<BusHeader> Filters { get { return _filters; } }
    }
}
