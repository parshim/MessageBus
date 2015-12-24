using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBus.Core
{
    /// <summary>
    /// Default task scheduler which executes all tasks in current thread
    /// </summary>
    public class SyncTaskScheduler : TaskScheduler
    {
        protected override void QueueTask(Task task)
        {
            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return Enumerable.Empty<Task>();
        }
    }
}