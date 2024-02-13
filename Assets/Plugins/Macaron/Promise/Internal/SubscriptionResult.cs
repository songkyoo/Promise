using System;

namespace Macaron.Internal
{
    internal struct SubscriptionResult<T>
    {
        public PromiseState State;
        public T Value;
        public Promise<T> Promise;
        public Exception Reason;

        public void ThrowIfNotValid()
        {
            switch (State)
            {
            case PromiseState.Fulfilled:
            case PromiseState.Cancelled:
                break;

            case PromiseState.Rejected:
                if (Reason == null)
                {
                    throw new InvalidOperationException();
                }
                break;

            default:
                throw new InvalidOperationException();
            }
        }
    }
}
