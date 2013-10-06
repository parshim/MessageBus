using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPublisher : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        void Send<TData>(TData data);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="busMessage"></param>
        void Send<TData>(BusMessage<TData> busMessage);
    }
}