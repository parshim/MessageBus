using System;
using System.Collections.Generic;

namespace MessageBus.Core.API
{
    /// <summary>
    /// 
    /// </summary>
    public class BusMessage
    {
        internal BusMessage()
        {
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// 
        /// </summary>
        public string BusId { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Sent { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, string> Headers { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BusMessage<T> : BusMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public T Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RawBusMessage : BusMessage
    {
        public string Name { get; internal set; }

        public string Namespace { get; internal set; }

        public object Data { get; internal set; }
    }
}