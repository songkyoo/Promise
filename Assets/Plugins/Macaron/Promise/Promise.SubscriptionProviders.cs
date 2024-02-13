using System;
using Macaron.Internal;

namespace Macaron
{
    partial class Promise<T>
    {
        private struct SubscriptionProvider_Then<TResult> : ISubscriptionProvider<T, TResult>
        {
            private readonly Func<T, TResult> _onFulfilled;
            private readonly Func<Exception, TResult> _onRejected;

            public SubscriptionProvider_Then(
                Func<T, TResult> onFulfilled,
                Func<Exception, TResult> onRejected)
            {
                if (onFulfilled == null)
                {
                    throw new ArgumentNullException("onFulfilled");
                }

                _onFulfilled = onFulfilled;
                _onRejected = onRejected;
            }

            #region Implementations of ISubscriptionProvider<T, TResult>
            public Subscription<T, TResult> Create(Promise<T> publisher, Promise<TResult> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_Then<TResult>(publisher, subscriber, _onFulfilled, _onRejected);
                }
                else
                {
                    return new Subscription_Cancelled<TResult>(publisher, subscriber);
                }
            }
            #endregion
        }

        private struct SubscriptionProvider_Catch<TException> : ISubscriptionProvider<T, T>
            where TException : Exception
        {
            private readonly Func<TException, bool> _predicate;
            private readonly Func<TException, T> _onRejected;

            public SubscriptionProvider_Catch(
                Func<TException, bool> predicate,
                Func<TException, T> onRejected)
            {
                if (predicate == null)
                {
                    throw new ArgumentNullException("predicate");
                }

                if (onRejected == null)
                {
                    throw new ArgumentNullException("onRejected");
                }

                _predicate = predicate;
                _onRejected = onRejected;
            }

            #region Implementations of ISubscriptionProvider<T, T>
            public Subscription<T, T> Create(Promise<T> publisher, Promise<T> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_Catch<TException>(publisher, subscriber, _predicate, _onRejected);
                }
                else
                {
                    return new Subscription_Cancelled<T>(publisher, subscriber);
                }
            }
            #endregion
        }

        private struct SubscriptionProvider_Then_Promise<TResult> : ISubscriptionProvider<T, TResult>
        {
            private readonly Func<T, Promise<TResult>> _onFulfilled;
            private readonly Func<Exception, Promise<TResult>> _onRejected;

            public SubscriptionProvider_Then_Promise(
                Func<T, Promise<TResult>> onFulfilled,
                Func<Exception, Promise<TResult>> onRejected)
            {
                if (onFulfilled == null)
                {
                    throw new ArgumentNullException("onFulfilled");
                }

                _onFulfilled = onFulfilled;
                _onRejected = onRejected;
            }

            #region Implementations of ISubscriptionProvider<T, TResult>
            public Subscription<T, TResult> Create(Promise<T> publisher, Promise<TResult> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_Then_Promise<TResult>(publisher, subscriber, _onFulfilled, _onRejected);
                }
                else
                {
                    return new Subscription_Cancelled<TResult>(publisher, subscriber);
                }
            }
            #endregion
        }

        private struct SubscriptionProvider_Catch_Promise<TException> : ISubscriptionProvider<T, T>
            where TException : Exception
        {
            private readonly Func<TException, bool> _predicate;
            private readonly Func<TException, Promise<T>> _onRejected;

            public SubscriptionProvider_Catch_Promise(
                Func<TException, bool> predicate,
                Func<TException, Promise<T>> onRejected)
            {
                if (predicate == null)
                {
                    throw new ArgumentNullException("predicate");
                }

                if (onRejected == null)
                {
                    throw new ArgumentNullException("onRejected");
                }

                _predicate = predicate;
                _onRejected = onRejected;
            }

            #region Implementations of ISubscriptionProvider<T, T>
            public Subscription<T, T> Create(Promise<T> publisher, Promise<T> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_Catch_Promise<TException>(publisher, subscriber, _predicate, _onRejected);
                }
                else
                {
                    return new Subscription_Cancelled<T>(publisher, subscriber);
                }
            }
            #endregion
        }

        private struct SubscriptionProvider_Then_NoReturn : ISubscriptionProvider<T, Nothing>
        {
            private readonly Action<T> _onFulfilled;
            private readonly Action<Exception> _onRejected;

            public SubscriptionProvider_Then_NoReturn(
                Action<T> onFulfilled,
                Action<Exception> onRejected)
            {
                if (onFulfilled == null)
                {
                    throw new ArgumentNullException("onFulfilled");
                }

                _onFulfilled = onFulfilled;
                _onRejected = onRejected;
            }

            #region Implementations of ISubscriptionProvider<T, Nothing>
            public Subscription<T, Nothing> Create(Promise<T> publisher, Promise<Nothing> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_Then_NoReturn(publisher, subscriber, _onFulfilled, _onRejected);
                }
                else
                {
                    return new Subscription_Cancelled<Nothing>(publisher, subscriber);
                }
            }
            #endregion
        }

        private struct SubscriptionProvider_Catch_NoReturn<TException> : ISubscriptionProvider<T, Nothing>
            where TException : Exception
        {
            private readonly Func<TException, bool> _predicate;
            private readonly Action<TException> _onRejected;

            public SubscriptionProvider_Catch_NoReturn(
                Func<TException, bool> predicate,
                Action<TException> onRejected)
            {
                if (predicate == null)
                {
                    throw new ArgumentNullException("predicate");
                }

                if (onRejected == null)
                {
                    throw new ArgumentNullException("onRejected");
                }

                _predicate = predicate;
                _onRejected = onRejected;
            }

            #region Implementations of ISubscriptionProvider<T, Nothing>
            public Subscription<T, Nothing> Create(Promise<T> publisher, Promise<Nothing> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_Catch_NoReturn<TException>(publisher, subscriber, _predicate, _onRejected);
                }
                else
                {
                    return new Subscription_Cancelled<Nothing>(publisher, subscriber);
                }
            }
            #endregion
        }

        private struct SubscriptionProvider_Finally : ISubscriptionProvider<T, T>
        {
            private readonly Action _onFinally;

            public SubscriptionProvider_Finally(Action onFinally)
            {
                if (onFinally == null)
                {
                    throw new ArgumentNullException("onFinally");
                }

                _onFinally = onFinally;
            }

            #region Implementations of ISubscriptionProvider<T, T>
            public Subscription<T, T> Create(Promise<T> publisher, Promise<T> subscriber)
            {
                return new Subscription_Finally(publisher, subscriber, _onFinally);
            }
            #endregion
        }

        private struct SubscriptionProvider_Resolve : ISubscriptionProvider<T, T>
        {
            #region Implementations of ISubscriptionProvider<T, T>
            public Subscription<T, T> Create(Promise<T> publisher, Promise<T> subscriber)
            {
                return new Subscription_Resolve(publisher, subscriber);
            }
            #endregion
        }

        private struct SubscriptionProvider_All : ISubscriptionProvider<T, T[]>
        {
            private readonly Aggregator _aggregator;
            private readonly int _index;

            public SubscriptionProvider_All(Aggregator aggregator, int index)
            {
                if (aggregator == null)
                {
                    throw new ArgumentNullException("aggregator");
                }

                if (index < 0)
                {
                    throw new ArgumentException(null, "index");
                }

                _aggregator = aggregator;
                _index = index;
            }

            #region Implementations of ISubscriptionProvider<T, T[]>
            public Subscription<T, T[]> Create(Promise<T> publisher, Promise<T[]> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_All(publisher, subscriber, _aggregator, _index);
                }
                else
                {
                    return new Subscription_Cancelled<T[]>(publisher, subscriber);
                }
            }
            #endregion
        }

        private struct SubscriptionProvider_Any : ISubscriptionProvider<T, T>
        {
            #region Implementations of ISubscriptionProvider<T, T>
            public Subscription<T, T> Create(Promise<T> publisher, Promise<T> subscriber)
            {
                if (publisher.State != PromiseState.Cancelled)
                {
                    return new Subscription_Resolve(publisher, subscriber);
                }
                else
                {
                    return new Subscription_Cancelled<T>(publisher, subscriber);
                }
            }
            #endregion
        }
    }
}
