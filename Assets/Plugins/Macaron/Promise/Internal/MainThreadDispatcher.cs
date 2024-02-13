using System;

namespace Macaron.Internal
{
    internal class MainThreadDispatcher : IDispatcher
    {
        public static readonly MainThreadDispatcher Instance = new MainThreadDispatcher();

        private MainThreadDispatcher()
        {
        }

        #region Implementations of IDispatcher
        public void Post(Action<object> action, object state)
        {
            global::Macaron.MainThreadDispatcher.Post(action, state);
        }
        #endregion
    }
}
