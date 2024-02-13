using System;

namespace Macaron.Internal
{
    internal class NullPromiseDispatcher : IDispatcher
    {
        public static readonly NullPromiseDispatcher Instance = new NullPromiseDispatcher();

        private NullPromiseDispatcher()
        {
        }

        #region Implementations of IDispatcher
        public void Post(Action<object> action, object state)
        {
        }
        #endregion
    }
}
