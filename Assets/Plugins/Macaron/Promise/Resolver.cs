using System;

namespace Macaron
{
    public struct Resolver<T>
    {
        private readonly IPromise _promise;
        private readonly Action<T> _resolve;
        private readonly Action<Exception> _reject;

        public Resolver(IPromise promise, Action<T> resolve, Action<Exception> reject)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (resolve == null)
            {
                throw new ArgumentNullException("resolve");
            }

            if (reject == null)
            {
                throw new ArgumentNullException("reject");
            }

            _promise = promise;
            _resolve = resolve;
            _reject = reject;
        }

        public PromiseState State
        {
            get { return _promise.State; }
        }

        public bool IsPending
        {
            get { return _promise.IsPending; }
        }

        public bool IsSettled
        {
            get { return _promise.IsSettled; }
        }

        public bool IsFulfilled
        {
            get { return _promise.IsFulfilled; }
        }

        public bool IsRejected
        {
            get { return _promise.IsRejected; }
        }

        public bool IsCancelled
        {
            get { return _promise.IsCancelled; }
        }

        public void Resolve(T value)
        {
            _resolve(value);
        }

        public void Reject(Exception reason)
        {
            _reject(reason);
        }
    }
}
