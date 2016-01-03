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
        void Send<TData>(TData data);

        /// <summary>
        /// Publish data message 
        /// </summary>
        /// <typeparam name="TData">Data type</typeparam>
        /// <param name="data">Data instance</param>
        /// <param name="persistant"></param>
        void Send<TData>(TData data, bool persistant);

        /// <summary>
        /// Publish data message 
        /// </summary>
        /// <typeparam name="TData">Data type</typeparam>
        /// <param name="data">Data instance</param>
        /// <param name="persistant"></param>
        /// <param name="priority">Message priority</param>
        void Send<TData>(TData data, bool persistant, byte priority);

        /// <summary>
        /// Publish bus message
        /// </summary>
        /// <typeparam name="TData">Data type</typeparam>
        /// <param name="busMessage">Bus message instance</param>
        void Send<TData>(BusMessage<TData> busMessage);

        /// <summary>
        /// Publish bus message
        /// </summary>
        /// <typeparam name="TData">Data type</typeparam>
        /// <param name="busMessage">Bus message instance</param>
        /// <param name="persistant"></param>
        void Send<TData>(BusMessage<TData> busMessage, bool persistant);

        /// <summary>
        /// Publish bus message
        /// </summary>
        /// <typeparam name="TData">Data type</typeparam>
        /// <param name="busMessage">Bus message instance</param>
        /// <param name="persistant"></param>
        /// <param name="priority">Message priority</param>
        void Send<TData>(BusMessage<TData> busMessage, bool persistant, byte priority);

        /// <summary>
        /// Publish raw bus message
        /// </summary>
        /// <param name="busMessage">Bus message instance</param>
        void Send(RawBusMessage busMessage);

        /// <summary>
        /// Publish raw bus message
        /// </summary>
        /// <param name="busMessage">Bus message instance</param>
        /// <param name="persistant"></param>
        void Send(RawBusMessage busMessage, bool persistant);

        /// <summary>
        /// Publish raw bus message
        /// </summary>
        /// <param name="busMessage">Bus message instance</param>
        /// <param name="persistant"></param>
        /// <param name="priority">Message priority</param>
        void Send(RawBusMessage busMessage, bool persistant, byte priority);
    }
}