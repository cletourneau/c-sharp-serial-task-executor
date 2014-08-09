using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Threading
{
    public class SerialTaskExecutor
    {
        private BlockingCollection<Action> actionQueue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

        public SerialTaskExecutor()
        {
            var worker = new Thread(Work) {Name = "SerialTaskExecutor"};
            worker.Start();
        }

        public void QueueTask(Action action)
        {
            actionQueue.Add(action);
        }

        private void Work()
        {
            while (true)
            {
                var action = actionQueue.Take();
                action.Invoke();
            }
        }

        public void ClearPendingTasks()
        {
            while (actionQueue.Count > 0)
            {
                Action a;
                actionQueue.TryTake(out a);
            }
        }
    }
}

