using System;
using System.Threading;
using Macaron.Internal;

namespace Macaron
{
    partial class Promise<T>
    {
        private void Publish<TPublisher>(Subscription<TPublisher, T> subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            if (!object.ReferenceEquals(this, subscription.Subscriber))
            {
                throw new ArgumentException(null, "subscription");
            }

            _subscriptionHandler.Run(Stubs<TPublisher>.Handle, subscription);
        }

        private void Handle<TPublisher>(Subscription<TPublisher, T> subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            int currentSettled = Interlocked.CompareExchange(ref _settled, Handling, Pending);

            if (currentSettled == Handling)
            {
                throw new InvalidOperationException();
            }

            if (currentSettled == Fulfilled || currentSettled == Rejected)
            {
                subscription.Cancel();
                return;
            }

            Promise<TPublisher> publisher = subscription.Publisher;
            bool synchronously = publisher._dispatcher.Equals(_dispatcher);

            if (currentSettled == Cancelled)
            {
                switch (publisher.State)
                {
                case PromiseState.Rejected:
                    publisher.Restore(subscription);
                    break;

                case PromiseState.Fulfilled:
                case PromiseState.Cancelled:
                    Apply(synchronously);
                    return;

                default:
                    throw new InvalidOperationException();
                }
            }

            var result = default(SubscriptionResult<T>);

            try
            {
                result = subscription.Handle();
            }
            catch (Exception e)
            {
                result.Reason = publisher.State == PromiseState.Rejected && publisher.Reason != e
                    ? new AggregateException(publisher.Reason, e)
                    : e;
                result.State = PromiseState.Rejected;
            }

            result.ThrowIfNotValid();

            switch (result.State)
            {
            case PromiseState.Fulfilled:
                Promise<T> promise = result.Promise;
                if (promise != null)
                {
                    _settled = Pending;

                    Resolve(promise);
                    return;
                }

                _settled = Fulfilled;
                _value = result.Value;
                break;

            case PromiseState.Rejected:
                _settled = Rejected;
                _reason = result.Reason;
                break;

            case PromiseState.Cancelled:
                _settled = Cancelled;

                Unbind();
                break;

            default:
                throw new InvalidOperationException();
            }

            Apply(synchronously);
        }

        private void Apply(bool synchronously)
        {
            if (False != Interlocked.Exchange(ref _applied, True))
            {
                return;
            }

            if (synchronously)
            {
                Apply();
            }
            else
            {
                _dispatcher.Post(Stubs.Apply, this);
            }
        }

        /// <summary>
        /// 프로미스의 상태를 확정시킨다.
        /// </summary>
        /// <remarks><see cref="Apply(bool)"/> 메서드에서만 호출되어야 한다.</remarks>
        private void Apply()
        {
            var state = (PromiseState)_settled;

            switch (state)
            {
            case PromiseState.Fulfilled:
            case PromiseState.Rejected:
                break;

            case PromiseState.Cancelled:
                try
                {
                    if (_onCancel != null)
                    {
                        _onCancel();
                    }
                }
                catch (Exception e)
                {
                    _reason = e;
                    state = PromiseState.Rejected;
                }
                break;

            default:
                throw new InvalidOperationException();
            }

            Subscriber? subscriberCopy = null;
            Subscriber[] subscriberCopies = null;

            lock (_subscriberManagementLock)
            {
                _state = state;

                if (_subscribers.Count == 1)
                {
                    subscriberCopy = _subscribers[0];
                }
                else if (_subscribers.Count > 1)
                {
                    subscriberCopies = _subscribers.ToArray();
                }
            }

            if (subscriberCopy != null)
            {
                subscriberCopy.Value.Subscription.Publish();
            }
            else if (subscriberCopies != null)
            {
                for (int i = 0; i < subscriberCopies.Length; ++i)
                {
                    subscriberCopies[i].Subscription.Publish();
                }
            }
        }
    }
}
