using System;

namespace MessageBus.Core.API
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class HeaderFilterAttribute : Attribute
    {
        public HeaderFilterAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Header name to filter on
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Acceptable header value
        /// </summary>
        public string Value { get; private set; }
    }
}