using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Publishes messages to the bus. 
    /// Publisher instances are not thread safe and therefor can not be shared between threads! Create new publisher for every publishing thread.
    /// </summary>
    public interface IPublisher : IDisposable
    {

        /// <summary>
        /// Publish data message 
        /// </summary>
        /// <typeparam name="TData">Data type</typeparam>
        /// <param name="data">Data instance</param>
        /// <param name="persistant"></param>
        void Send<TData>(TData data, bool persistant = false);

        /// <summary>
        /// Publish bus message
        /// </summary>
        /// <typeparam name="TData">Data type</typeparam>
        /// <param name="busMessage">Bus message instance</param>
        /// <param name="persistant"></param>
        void Send<TData>(BusMessage<TData> busMessage, bool persistant = false);

        /// <summary>
        /// Publish raw bus message
        /// </summary>
        /// <param name="busMessage">Bus message instance</param>
        /// <param name="persistant"></param>
        void Send(RawBusMessage busMessage, bool persistant = false);
    }
}