using System;
using System.Collections.Generic;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Bus message header
    /// </summary>
    public class BusHeader : BusHeaderBase
    {
        public BusHeader()
        {
        }

        public BusHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Header value
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Bus message
    /// </summary>
    public class BusMessage
    {
        internal BusMessage()
        {
            Headers = new List<BusHeaderBase>();
        }

        internal BusMessage(params BusHeader[] headers)
        {
            Headers = new List<BusHeaderBase>(headers);
        }

        internal BusMessage(IEnumerable<BusHeader> headers)
        {
            Headers = new List<BusHeaderBase>(headers);
        }

        /// <summary>
        /// Message originator Bus Id
        /// </summary>
        public string BusId { get; internal set; }

        /// <summary>
        /// Date and time when message were sent
        /// </summary>
        public DateTime Sent { get; internal set; }

        /// <summary>
        /// List of headers associated with the message
        /// </summary>
        public IList<BusHeaderBase> Headers { get; private set; }
    }

    /// <summary>
    /// Bus message with data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BusMessage<T> : BusMessage
    {
        /// <summary>
        /// Data payload
        /// </summary>
        public T Data { get; set; }
    }

    /// <summary>
    /// Raw data message
    /// </summary>
    public class RawBusMessage : BusMessage
    {
        /// <summary>
        /// Data payload contact name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Data payload contact namespace name
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Data payload
        /// </summary>
        public object Data { get; set; }
    }
}