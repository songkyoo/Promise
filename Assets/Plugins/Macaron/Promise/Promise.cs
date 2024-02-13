using System;
using System.Collections.Generic;
using System.Threading;
using Macaron.Internal;

namespace Macaron
{
    public enum PromiseState
    {
        Pending,
        Fulfilled,
        Rejected,
        Cancelled
    }

    public partial class Promise<T> : IPromise, IPromise<T>, ICancelable
    {
        #region Constants
        private const bool Asynchronously = false;
        private const int False = 0;
        private const int True = 1;
        private const int Pending = (int)PromiseState.Pending;
        private const int Fulfilled = (int)PromiseState.Fulfilled;
        private const int Rejected = (int)PromiseState.Rejected;
        private const int Cancelled = (int)PromiseState.Cancelled;
        private const int Handling = -1;
        #endregion

        #region Fields
        private readonly IDispatcher _dispatcher;
        private readonly Action _onCancel;
        private bool _ignoreUnhandledRejection;

        private int _settled;
        private int _applied;
        private T _value;
        private Exception _reason;

        private readonly object _subscriberManagementLock;
        private readonly List<Subscriber> _subscribers;
        private PromiseState _state;

        private readonly object _publisherManagementLock;
        private readonly List<ICancelable> _publishers;
        private bool _unbinded;

        private readonly SubscriptionHandler _subscriptionHandler;
        #endregion

        #region Constructors
        public Promise(IDispatcher dispatcher, Func<Action<T>, Action<Exception>, Action> executor)
            : this(dispatcher)
        {
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }

            try
            {
                _onCancel = executor(Resolve, Reject);
            }
            catch (Exception e)
            {
                if (_state == PromiseState.Pending)
                {
                    Reject(e);
                }
            }
        }

        public Promise(IDispatcher dispatcher, Func<Action<Promise<T>>, Action<Exception>, Action> executor)
            : this(dispatcher)
        {
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }

            try
            {
                _onCancel = executor(Resolve, Reject);
            }
            catch (Exception e)
            {
                if (_state == PromiseState.Pending)
                {
                    Reject(e);
                }
            }
        }

        public Promise(IDispatcher dispatcher, Action<Action<T>, Action<Exception>> executor)
            : this(dispatcher)
        {
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }

            try
            {
                executor(Resolve, Reject);
            }
            catch (Exception e)
            {
                if (_state == PromiseState.Pending)
                {
                    Reject(e);
                }
            }
        }

        public Promise(IDispatcher dispatcher, Action<Action<Promise<T>>, Action<Exception>> executor)
            : this(dispatcher)
        {
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }

            try
            {
                executor(Resolve, Reject);
            }
            catch (Exception e)
            {
                if (_state == PromiseState.Pending)
                {
                    Reject(e);
                }
            }
        }

        private Promise(IDispatcher dispatcher, Action onCancel)
            : this(dispatcher)
        {
            _onCancel = onCancel;
        }

        private Promise(IDispatcher dispatcher)
            : this(dispatcher, PromiseState.Pending, default(T), null)
        {
        }

        private Promise(IDispatcher dispatcher, PromiseState state, T value, Exception reason)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            switch (state)
            {
            case PromiseState.Pending:
            case PromiseState.Cancelled:
                break;

            case PromiseState.Fulfilled:
                _value = value;
                break;

            case PromiseState.Rejected:
                if (reason == null)
                {
                    throw new ArgumentNullException("reason");
                }

                _reason = reason;
                break;

            default:
                throw new ArgumentException(null, "state");
            }

            _dispatcher = dispatcher;
            _settled = (int)state;
            _applied = False;

            _subscriberManagementLock = new object();
            _subscribers = new List<Subscriber>(1);
            _state = state;

            _publisherManagementLock = new object();
            _publishers = new List<ICancelable>(1);

            _subscriptionHandler = new SubscriptionHandler();
        }
        #endregion

        ~Promise()
        {
            if (_ignoreUnhandledRejection || _state != PromiseState.Rejected)
            {
                return;
            }

            if (!HasSubscriber)
            {
                Promise.HandleUnhandledRejection(_reason);
            }
        }

        #region Implementations of IPromise
        public PromiseState State
        {
            get { return _state; }
        }

        public bool IsPending
        {
            get { return _state == PromiseState.Pending; }
        }

        public bool IsSettled
        {
            get
            {
                return
                    _state == PromiseState.Fulfilled ||
                    _state == PromiseState.Rejected ||
                    _state == PromiseState.Cancelled;
            }
        }

        public bool IsFulfilled
        {
            get { return _state == PromiseState.Fulfilled; }
        }

        public bool IsRejected
        {
            get { return _state == PromiseState.Rejected; }
        }

        public bool IsCancelled
        {
            get { return _state == PromiseState.Cancelled; }
        }

        public Exception Reason
        {
            get
            {
                if (_state != PromiseState.Rejected)
                {
                    throw new InvalidOperationException();
                }

                return _reason;
            }
        }
        #endregion

        #region Implementations of IPromise<T>
        public T Value
        {
            get
            {
                if (_state != PromiseState.Fulfilled)
                {
                    throw new InvalidOperationException();
                }

                return _value;
            }
        }
        #endregion

        #region Implementations of ICancelable
        public void Cancel()
        {
            switch (Interlocked.CompareExchange(ref _settled, Cancelled, Pending))
            {
            case Handling:
                _subscriptionHandler.Run(Stubs.Cancel, this);
                break;

            case Pending:
                Stubs.Cancel(this);
                break;

            case Fulfilled:
            case Rejected:
            case Cancelled:
                break;

            default:
                throw new InvalidOperationException();
            }
        }
        #endregion

        #region Properties
        public bool IgnoreUnhandledRejection
        {
            get { return _ignoreUnhandledRejection; }
            set { _ignoreUnhandledRejection = value; }
        }
        #endregion

        #region Methods
        public Promise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Func<Exception, TResult> onRejected = null)
        {
            var subscriber = new Promise<TResult>(_dispatcher);
            var provider = new SubscriptionProvider_Then<TResult>(onFulfilled, onRejected);

            return Register(subscriber, provider);
        }

        public Promise<T> Catch<TException>(Func<TException, bool> predicate, Func<TException, T> onRejected)
            where TException : Exception
        {
            var subscriber = new Promise<T>(_dispatcher);
            var provider = new SubscriptionProvider_Catch<TException>(predicate, onRejected);

            return Register(subscriber, provider);
        }

        public Promise<T> Catch<TException>(Func<TException, T> onRejected)
            where TException : Exception
        {
            return Catch(Predicates<TException>.True, onRejected);
        }

        public Promise<TResult> Then<TResult>(
            Func<T, Promise<TResult>> onFulfilled,
            Func<Exception, Promise<TResult>> onRejected = null)
        {
            var subscriber = new Promise<TResult>(_dispatcher);
            var provider = new SubscriptionProvider_Then_Promise<TResult>(onFulfilled, onRejected);

            return Register(subscriber, provider);
        }

        public Promise<T> Catch<TException>(Func<TException, bool> predicate, Func<TException, Promise<T>> onRejected)
            where TException : Exception
        {
            var subscriber = new Promise<T>(_dispatcher);
            var provider = new SubscriptionProvider_Catch_Promise<TException>(predicate, onRejected);

            return Register(subscriber, provider);
        }

        public Promise<T> Catch<TException>(Func<TException, Promise<T>> onRejected)
            where TException : Exception
        {
            return Catch(Predicates<TException>.True, onRejected);
        }

        public Promise<Nothing> Then(Action<T> onFulfilled, Action<Exception> onRejected = null)
        {
            var subscriber = new Promise<Nothing>(_dispatcher);
            var provider = new SubscriptionProvider_Then_NoReturn(onFulfilled, onRejected);

            return Register(subscriber, provider);
        }

        public Promise<Nothing> Catch<TException>(Func<TException, bool> predicate, Action<TException> onRejected)
            where TException : Exception
        {
            var subscriber = new Promise<Nothing>(_dispatcher);
            var provider = new SubscriptionProvider_Catch_NoReturn<TException>(predicate, onRejected);

            return Register(subscriber, provider);
        }

        public Promise<Nothing> Catch<TException>(Action<TException> onRejected)
            where TException : Exception
        {
            return Catch(Predicates<TException>.True, onRejected);
        }

        public Promise<T> Finally(Action onFinally)
        {
            var subscriber = new Promise<T>(_dispatcher, onFinally);
            var provider = new SubscriptionProvider_Finally(onFinally);

            return Register(subscriber, provider);
        }

        private void Resolve(T value)
        {
            if (Pending != Interlocked.CompareExchange(ref _settled, Fulfilled, Pending))
            {
                return;
            }

            _value = value;

            Apply(Asynchronously);
        }

        private void Resolve(Promise<T> promise)
        {
            if (promise == null)
            {
                Resolve(default(T));
                return;
            }

            promise.Register(this, new SubscriptionProvider_Resolve());
        }

        private void Reject(Exception reason)
        {
            if (reason == null)
            {
                throw new ArgumentNullException("reason");
            }

            if (Pending != Interlocked.CompareExchange(ref _settled, Rejected, Pending))
            {
                return;
            }

            _reason = reason;

            Apply(Asynchronously);
        }
        #endregion
    }
}
