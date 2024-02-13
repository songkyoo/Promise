using System;

namespace Macaron
{
    public interface IPromise
    {
        PromiseState State
        {
            get;
        }

        bool IsPending
        {
            get;
        }

        bool IsSettled
        {
            get;
        }

        bool IsFulfilled
        {
            get;
        }

        bool IsRejected
        {
            get;
        }

        bool IsCancelled
        {
            get;
        }

        Exception Reason
        {
            get;
        }
    }

    public interface IPromise<T> : IPromise
    {
        T Value
        {
            get;
        }
    }
}
