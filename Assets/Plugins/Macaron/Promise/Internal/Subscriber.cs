using System;

namespace Macaron.Internal
{
    internal struct Subscriber : IEquatable<Subscriber>
    {
        public readonly ISubscription Subscription;
        public bool Cancelled;

        public Subscriber(ISubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            Subscription = subscription;
            Cancelled = false;
        }

        #region Implementations of IEquatable<SubscriptionInfo>
        public bool Equals(Subscriber other)
        {
            return Subscription == other.Subscription;
        }
        #endregion
    }
}
