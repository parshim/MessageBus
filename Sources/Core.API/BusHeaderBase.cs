namespace MessageBus.Core.API
{
    public abstract class BusHeaderBase
    {
        /// <summary>
        /// Header name
        /// </summary>
        public string Name { get; set; }

        public abstract object GetValue();
    }
}