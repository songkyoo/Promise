using System;
using System.Collections.Generic;

namespace Macaron.Internal
{
    internal class ThreadSafeQueueWorker
    {
        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        #region Fields
        private readonly object _queueLock = new object();

        private List<Task> _tasks;
        private List<Task> _runningTasks;
        #endregion

        #region Constructors
        public ThreadSafeQueueWorker()
        {
            _tasks = new List<Task>();
            _runningTasks = new List<Task>();
        }

        public ThreadSafeQueueWorker(int capacity)
        {
            _tasks = new List<Task>(capacity);
            _runningTasks = new List<Task>(capacity);
        }
        #endregion

        public void Enqueue(Action<object> action, object state)
        {
            var task = new Task(action, state);

            lock (_queueLock)
            {
                _tasks.Add(task);
            }
        }

        public void Run(Action<Exception> unhandledExceptionHandler)
        {
            lock (_queueLock)
            {
                if (_tasks.Count == 0)
                {
                    return;
                }

                Swap(ref _tasks, ref _runningTasks);
            }

            for (int i = 0; i < _runningTasks.Count; ++i)
            {
                try
                {
                    _runningTasks[i].Run();
                }
                catch (Exception e)
                {
                    if (unhandledExceptionHandler == null)
                    {
                        throw;
                    }

                    unhandledExceptionHandler.Invoke(e);
                }
            }

            _runningTasks.Clear();
        }
    }
}
