using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Binding.RabbitMQ.Clent.Extensions
{
    public interface IMessageQueue
    {
        /// <summary>
        /// Waits till queue will receive any message. Blocks calling thread till queue have at least one message or till timeout.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>True if queue have at least one message at method call or till timeout any message enquired</returns>
        bool WaitForMessage(TimeSpan timeout);

        ///<summary>Retrieve the first item from the queue, or return
        ///nothing if no items are available after the given
        ///timeout</summary>
        ///<remarks>
        ///<para>
        /// If one or more items are present on the queue at the time
        /// the call is made, the call will return
        /// immediately. Otherwise, the calling thread blocks until
        /// either an item appears on the queue, or
        /// millisecondsTimeout milliseconds have elapsed.
        ///</para>
        ///<para>
        /// Returns true in the case that an item was available before
        /// the timeout, in which case the out parameter "result" is
        /// set to the item itself.
        ///</para>
        ///<para>
        /// If no items were available before the timeout, returns
        /// false, and sets "result" to null.
        ///</para>
        ///<para>
        /// A timeout of -1 (i.e. System.Threading.Timeout.Infinite)
        /// will be interpreted as a command to wait for an
        /// indefinitely long period of time for an item to become
        /// available. Usage of such a timeout is equivalent to
        /// calling Dequeue() with no arguments. See also the MSDN
        /// documentation for
        /// System.Threading.Monitor.Wait(object,int).
        ///</para>
        ///<para>
        /// If no items are present and the queue is in a closed
        /// state, or if at any time while waiting the queue
        /// transitions to a closed state (by a call to Close()), this
        /// method will throw EndOfStreamException.
        ///</para>
        ///</remarks>
        bool Dequeue(TimeSpan timeout, out BasicDeliverEventArgs message);

        ///<summary>Retrieve the first item from the queue, or return
        ///defaultValue immediately if no items are available
        ///</summary>
        ///<remarks>
        ///<para>
        /// If one or more objects are present in the queue at the
        /// time of the call, the first item is removed from the queue
        /// and returned. Otherwise, the defaultValue that was passed
        /// in is returned immediately. This defaultValue may be null,
        /// or in cases where null is part of the range of the queue,
        /// may be some other sentinel object. The difference between
        /// DequeueNoWait() and Dequeue() is that DequeueNoWait() will
        /// not block when no items are available in the queue,
        /// whereas Dequeue() will.
        ///</para>
        ///<para>
        /// If at the time of call the queue is empty and in a
        /// closed state (following a call to Close()), then this
        /// method will throw EndOfStreamException.
        ///</para>
        ///</remarks>
        BasicDeliverEventArgs DequeueNoWait();

        ///<summary>Retrieve the first item from the queue, or block if none available</summary>
        ///<remarks>
        ///Callers of Dequeue() will block if no items are available
        ///until some other thread calls Enqueue() or the queue is
        ///closed. In the latter case this method will throw
        ///EndOfStreamException.
        ///</remarks>
        BasicDeliverEventArgs Dequeue();

        ///<summary>Retrieve the consumer tag this consumer is
        ///registered as; to be used when discussing this consumer
        ///with the server, for instance with
        ///IModel.BasicCancel().</summary>
        string ConsumerTag { get; set; }
        
        ///<summary>If our IModel shuts down, this property will
        ///contain a description of the reason for the
        ///shutdown. Otherwise it will contain null. See
        ///ShutdownEventArgs.</summary>
        ShutdownEventArgs ShutdownReason { get; }
    }
}