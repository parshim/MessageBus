using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISubscriber : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="callback"></param>
        /// <param name="hierarchy"></param>
        /// <param name="receiveSelfPublish"></param>
        /// <returns></returns>
        bool Subscribe<TData>(Action<TData> callback, bool hierarchy = false, bool receiveSelfPublish = false);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="callback"></param>
        /// <param name="hierarchy"></param>
        /// <param name="receiveSelfPublish"></param>
        /// <returns></returns>
        bool Subscribe<TData>(Action<BusMessage<TData>> callback, bool hierarchy = false, bool receiveSelfPublish = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="callback"></param>
        /// <param name="hierarchy"></param>
        /// <param name="receiveSelfPublish"></param>
        /// <returns></returns>
        bool Subscribe(Type dataType, Action<object> callback, bool hierarchy = false, bool receiveSelfPublish = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="callback"></param>
        /// <param name="hierarchy"></param>
        /// <param name="receiveSelfPublish"></param>
        /// <returns></returns>
        bool Subscribe(Type dataType, Action<RawBusMessage> callback, bool hierarchy = false, bool receiveSelfPublish = false);
    }
}