using System;
using System.Collections.Generic;

namespace Macaron.Internal
{
    internal class SubscriptionHandler
    {
        private readonly object _lock = new object();
        private Queue<Task> _queue;
        private int _count;

        public void Run(Action<object> action, object state)
        {
            var task = new Task(action, state);

            lock (_lock)
            {
                _count += 1;

                if (_count > 1)
                {
                    if (_queue == null)
                    {
                        _queue = new Queue<Task>(1);
                    }

                    _queue.Enqueue(task);
                    return;
                }
            }

            for (; ; )
            {
                task.Run();

                lock (_lock)
                {
                    _count -= 1;

                    if (_count == 0)
                    {
                        return;
                    }

                    task = _queue.Dequeue();
                }
            }
        }
    }
}
