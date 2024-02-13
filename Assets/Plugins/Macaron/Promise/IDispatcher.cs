using System;

namespace Macaron
{
    public interface IDispatcher
    {
        void Post(Action<object> action, object state);
    }
}
