using System;

namespace Macaron.Internal
{
    internal struct Task
    {
        private readonly Action<object> _action;
        private readonly object _state;

        public Task(Action<object> action, object state)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = action;
            _state = state;
        }

        public void Run()
        {
            _action(_state);
        }
    }
}
