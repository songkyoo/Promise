using System;
using Macaron.Internal;

namespace Macaron
{
    partial class Promise<T>
    {
        private bool HasSubscriber
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _subscribers.Count; ++i)
                {
                    if (!_subscribers[i].Cancelled)
                    {
                        count += 1;
                    }
                }

                return count > 0;
            }
        }

        private Promise<TSubscriber> Register<TSubscriber, TSubscriptionProvider>(
            Promise<TSubscriber> subscriber,
            TSubscriptionProvider provider)
            where TSubscriptionProvider : struct, ISubscriptionProvider<T, TSubscriber>
        {
            Subscription<T, TSubscriber> subscription;
            bool shouldPublish;

            lock (_subscriberManagementLock)
            {
                switch (_state)
                {
                case PromiseState.Pending:
                case PromiseState.Fulfilled:
                case PromiseState.Rejected:
                case PromiseState.Cancelled:
                    subscription = provider.Create(this, subscriber);
                    break;

                default:
                    throw new InvalidOperationException();
                }

                var item = new Subscriber(subscription);

                if (_subscribers.Contains(item))
                {
                    throw new ArgumentException(null, "subscriber");
                }

                _subscribers.Add(item);

                shouldPublish = _state != PromiseState.Pending;
            }

            if (shouldPublish)
            {
                _dispatcher.Post(Stubs.Publish, subscription);
            }

            subscriber.Bind(subscription);

            return subscriber;
        }

        private void Cancel(ISubscription subscription)
        {
            var subscriber = new Subscriber(subscription) { Cancelled = true };
            bool shouldCancel;

            lock (_subscriberManagementLock)
            {
                int index = _subscribers.IndexOf(subscriber);

                if (index == -1 || _subscribers[index].Cancelled)
                {
                    throw new ArgumentException(null, "subscription");
                }

                _subscribers[index] = subscriber;

                if (_state != PromiseState.Pending)
                {
                    return;
                }

                shouldCancel = !HasSubscriber;
            }

            if (shouldCancel)
            {
                Cancel();
            }
        }

        private void Restore(ISubscription subscription)
        {
            var subscriber = new Subscriber(subscription);

            lock (_subscriberManagementLock)
            {
                int index = _subscribers.IndexOf(subscriber);

                if (index == -1 || !_subscribers[index].Cancelled)
                {
                    throw new ArgumentException(null, "subscription");
                }

                _subscribers[index] = subscriber;
            }
        }
    }
}
