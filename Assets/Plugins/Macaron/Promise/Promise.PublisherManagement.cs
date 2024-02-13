using System;
using Macaron.Internal;

namespace Macaron
{
    partial class Promise<T>
    {
        private void Bind(ICancelable subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            bool shouldCancel;

            lock (_publisherManagementLock)
            {
                if (_publishers.Contains(subscription))
                {
                    throw new ArgumentException(null, "subscription");
                }

                _publishers.Add(subscription);

                shouldCancel = _unbinded;
            }

            if (shouldCancel)
            {
                subscription.Cancel();
            }
        }

        private bool Unbind()
        {
            ICancelable publisherCopy = null;
            ICancelable[] publisherCopies = null;

            lock (_publisherManagementLock)
            {
                if (_unbinded)
                {
                    throw new InvalidOperationException();
                }

                _unbinded = true;

                if (_publishers.Count == 1)
                {
                    publisherCopy = _publishers[0];
                }
                else if (_publishers.Count > 1)
                {
                    publisherCopies = _publishers.ToArray();
                }
                else
                {
                    return false;
                }
            }

            if (publisherCopy != null)
            {
                publisherCopy.Cancel();
            }
            else if (publisherCopies != null)
            {
                for (int i = 0; i < publisherCopies.Length; ++i)
                {
                    publisherCopies[i].Cancel();
                }
            }

            return true;
        }
    }
}
