using System;

namespace Macaron.Internal
{
    internal abstract class Subscription<TPublisher, TSubscriber> : ISubscription, ICancelable
    {
        private readonly Promise<TPublisher> _publisher;
        private readonly Promise<TSubscriber> _subscriber;

        protected Subscription(Promise<TPublisher> publisher, Promise<TSubscriber> subscriber)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException("publisher");
            }

            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            if (object.ReferenceEquals(publisher, subscriber))
            {
                throw new ArgumentException(null, "subscriber");
            }

            _publisher = publisher;
            _subscriber = subscriber;
        }

        #region Implementations of ISubscription
        public abstract void Publish();
        #endregion

        #region Implementations of ICancelable
        public abstract void Cancel();
        #endregion

        public Promise<TPublisher> Publisher
        {
            get { return _publisher; }
        }

        public Promise<TSubscriber> Subscriber
        {
            get { return _subscriber; }
        }

        public abstract SubscriptionResult<TSubscriber> Handle();
    }
}
