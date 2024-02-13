using System;
using System.Threading;
using Macaron.Internal;

namespace Macaron
{
    partial class Promise<T>
    {
        private abstract class Subscription<TResult> : Subscription<T, TResult>
        {
            protected Subscription(Promise<T> publisher, Promise<TResult> subscriber)
                : base(publisher, subscriber)
            {
            }

            #region Overrides
            public override void Publish()
            {
                Subscriber.Publish(this);
            }

            public override void Cancel()
            {
                Publisher.Cancel(this);
            }

            public abstract override SubscriptionResult<TResult> Handle();
            #endregion
        }

        private class Subscription_Then<TResult> : Subscription<TResult>
        {
            private readonly Func<T, TResult> _onFulfilled;
            private readonly Func<Exception, TResult> _onRejected;

            public Subscription_Then(
                Promise<T> publisher,
                Promise<TResult> subscriber,
                Func<T, TResult> onFulfilled,
                Func<Exception, TResult> onRejected)
                : base(publisher, subscriber)
            {
                _onFulfilled = onFulfilled;
                _onRejected = onRejected;
            }

            #region Overrides
            public override SubscriptionResult<TResult> Handle()
            {
                PromiseState state = Publisher.State;
                TResult value = default(TResult);
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    value = _onFulfilled(Publisher.Value);
                    break;

                case PromiseState.Rejected:
                    if (_onRejected == null)
                    {
                        reason = Publisher.Reason;
                    }
                    else
                    {
                        value = _onRejected(Publisher.Reason);
                        state = PromiseState.Fulfilled;
                    }
                    break;
                }

                return new SubscriptionResult<TResult>
                {
                    State = state,
                    Value = value,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_Catch<TException> : Subscription<T>
            where TException : Exception
        {
            private readonly Func<TException, bool> _predicate;
            private readonly Func<TException, T> _onRejected;

            public Subscription_Catch(
                Promise<T> publisher,
                Promise<T> subscriber,
                Func<TException, bool> predicate,
                Func<TException, T> onRejected)
                : base(publisher, subscriber)
            {
                _predicate = predicate;
                _onRejected = onRejected;
            }

            #region Overrides
            public override SubscriptionResult<T> Handle()
            {
                PromiseState state = Publisher.State;
                T value = default(T);
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    value = Publisher.Value;
                    break;

                case PromiseState.Rejected:
                    Exception e = Publisher.Reason;
                    if (e is TException && _predicate((TException)e))
                    {
                        value = _onRejected((TException)e);
                        state = PromiseState.Fulfilled;
                    }
                    else
                    {
                        reason = e;
                    }
                    break;
                }

                return new SubscriptionResult<T>
                {
                    State = state,
                    Value = value,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_Then_Promise<TResult> : Subscription<TResult>
        {
            private readonly Func<T, Promise<TResult>> _onFulfilled;
            private readonly Func<Exception, Promise<TResult>> _onRejected;

            public Subscription_Then_Promise(
                Promise<T> publisher,
                Promise<TResult> subscriber,
                Func<T, Promise<TResult>> onFulfilled,
                Func<Exception, Promise<TResult>> onRejected)
                : base(publisher, subscriber)
            {
                _onFulfilled = onFulfilled;
                _onRejected = onRejected;
            }

            #region Overrides
            public override SubscriptionResult<TResult> Handle()
            {
                PromiseState state = Publisher.State;
                Promise<TResult> promise = null;
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    promise = _onFulfilled(Publisher.Value);
                    break;

                case PromiseState.Rejected:
                    if (_onRejected == null)
                    {
                        reason = Publisher.Reason;
                    }
                    else
                    {
                        promise = _onRejected(Publisher.Reason);
                        state = PromiseState.Fulfilled;
                    }
                    break;
                }

                return new SubscriptionResult<TResult>
                {
                    State = state,
                    Promise = promise,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_Catch_Promise<TException> : Subscription<T>
            where TException : Exception
        {
            private readonly Func<TException, bool> _predicate;
            private readonly Func<TException, Promise<T>> _onRejected;

            public Subscription_Catch_Promise(
                Promise<T> publisher,
                Promise<T> subscriber,
                Func<TException, bool> predicate,
                Func<TException, Promise<T>> onRejected)
                : base(publisher, subscriber)
            {
                _predicate = predicate;
                _onRejected = onRejected;
            }

            #region Overrides
            public override SubscriptionResult<T> Handle()
            {
                PromiseState state = Publisher.State;
                T value = default(T);
                Promise<T> promise = null;
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    value = Publisher.Value;
                    break;

                case PromiseState.Rejected:
                    Exception e = Publisher.Reason;
                    if (e is TException && _predicate((TException)e))
                    {
                        promise = _onRejected((TException)e);
                        state = PromiseState.Fulfilled;
                    }
                    else
                    {
                        reason = e;
                    }
                    break;
                }

                return new SubscriptionResult<T>
                {
                    State = state,
                    Value = value,
                    Promise = promise,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_Then_NoReturn : Subscription<Nothing>
        {
            private readonly Action<T> _onFulfilled;
            private readonly Action<Exception> _onRejected;

            public Subscription_Then_NoReturn(
                Promise<T> publisher,
                Promise<Nothing> subscriber,
                Action<T> onFulfilled,
                Action<Exception> onRejected)
                : base(publisher, subscriber)
            {
                _onFulfilled = onFulfilled;
                _onRejected = onRejected;
            }

            #region Overrides
            public override SubscriptionResult<Nothing> Handle()
            {
                PromiseState state = Publisher.State;
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    _onFulfilled(Publisher.Value);
                    break;

                case PromiseState.Rejected:
                    if (_onRejected == null)
                    {
                        reason = Publisher.Reason;
                    }
                    else
                    {
                        _onRejected(Publisher.Reason);
                        state = PromiseState.Fulfilled;
                    }
                    break;
                }

                return new SubscriptionResult<Nothing>
                {
                    State = state,
                    Value = Nothing.Value,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_Catch_NoReturn<TException> : Subscription<Nothing>
            where TException : Exception
        {
            private readonly Func<TException, bool> _predicate;
            private readonly Action<TException> _onRejected;

            public Subscription_Catch_NoReturn(
                Promise<T> publisher,
                Promise<Nothing> subscriber,
                Func<TException, bool> predicate,
                Action<TException> onRejected)
                : base(publisher, subscriber)
            {
                _predicate = predicate;
                _onRejected = onRejected;
            }

            #region Overrides
            public override SubscriptionResult<Nothing> Handle()
            {
                PromiseState state = Publisher.State;
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    break;

                case PromiseState.Rejected:
                    Exception e = Publisher.Reason;
                    if (e is TException && _predicate((TException)e))
                    {
                        _onRejected((TException)e);
                        state = PromiseState.Fulfilled;
                    }
                    else
                    {
                        reason = e;
                    }
                    break;
                }

                return new SubscriptionResult<Nothing>
                {
                    State = state,
                    Value = Nothing.Value,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_Cancelled<TResult> : Subscription<TResult>
        {
            public Subscription_Cancelled(Promise<T> publisher, Promise<TResult> subscriber)
                : base(publisher, subscriber)
            {
            }

            #region Overrides
            public override SubscriptionResult<TResult> Handle()
            {
                return new SubscriptionResult<TResult>
                {
                    State = PromiseState.Rejected,
                    Reason = new CancellationException()
                };
            }
            #endregion
        }

        private class Subscription_Finally : Subscription<T>
        {
            private readonly Action _onFinally;

            public Subscription_Finally(Promise<T> publisher, Promise<T> subscriber, Action onFinally)
                : base(publisher, subscriber)
            {
                _onFinally = onFinally;
            }

            #region Overrides
            public override SubscriptionResult<T> Handle()
            {
                PromiseState state = Publisher.State;
                T value = default(T);
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    _onFinally();
                    value = Publisher.Value;
                    break;

                case PromiseState.Rejected:
                    _onFinally();
                    reason = Publisher.Reason;
                    break;
                }

                return new SubscriptionResult<T>
                {
                    State = state,
                    Value = value,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_Resolve : Subscription<T>
        {
            public Subscription_Resolve(Promise<T> publisher, Promise<T> subscriber)
                : base(publisher, subscriber)
            {
            }

            #region Overrides
            public override SubscriptionResult<T> Handle()
            {
                PromiseState state = Publisher.State;
                T value = default(T);
                Exception reason = null;

                switch (state)
                {
                case PromiseState.Fulfilled:
                    value = Publisher.Value;
                    break;

                case PromiseState.Rejected:
                    reason = Publisher.Reason;
                    break;
                }

                return new SubscriptionResult<T>
                {
                    State = state,
                    Value = value,
                    Reason = reason
                };
            }
            #endregion
        }

        private class Subscription_All : Subscription<T[]>
        {
            private readonly Aggregator _aggregator;
            private readonly int _index;
            private int _resolved = False;
            private SubscriptionResult<T[]> _result;

            public Subscription_All(Promise<T> publisher, Promise<T[]> subscriber, Aggregator aggregator, int index)
                : base(publisher, subscriber)
            {
                _aggregator = aggregator;
                _index = index;
            }

            #region Overrides
            public override void Publish()
            {
                bool completed = false;

                switch (Publisher.State)
                {
                case PromiseState.Fulfilled:
                    _aggregator.Values[_index] = Publisher.Value;

                    int completedCount = Interlocked.Increment(ref _aggregator.CompletedCount);
                    if (completedCount == _aggregator.Count && False == Interlocked.Exchange(ref _resolved, True))
                    {
                        _result = new SubscriptionResult<T[]>
                        {
                            State = PromiseState.Fulfilled,
                            Value = _aggregator.Values
                        };
                        completed = true;
                    }
                    break;

                case PromiseState.Rejected:
                    if (False == Interlocked.Exchange(ref _resolved, True))
                    {
                        _result = new SubscriptionResult<T[]>
                        {
                            State = PromiseState.Rejected,
                            Reason = Publisher.Reason
                        };
                        completed = true;
                    }
                    break;

                case PromiseState.Cancelled:
                    if (False == Interlocked.Exchange(ref _resolved, True))
                    {
                        _result = new SubscriptionResult<T[]>
                        {
                            State = PromiseState.Cancelled
                        };
                        completed = true;
                    }
                    break;

                default:
                    throw new InvalidOperationException();
                }

                if (completed)
                {
                    Subscriber.Publish(this);
                }
            }

            public override SubscriptionResult<T[]> Handle()
            {
                return _result;
            }
            #endregion
        }
    }
}
