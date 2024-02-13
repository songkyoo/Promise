using System;
using System.Collections;
using System.Collections.Generic;

namespace Macaron.Internal
{
    internal static class PromiseYieldInstruction
    {
        public static IEnumerator GetEnumerator(IPromise promise)
        {
            while (promise.IsPending)
            {
                yield return null;
            }
        }

        public static IEnumerator GetEnumerator(IPromise promise, ICollection<IPromise> promises)
        {
            IPromise[] promiseCopies = null;

            if (promises != null && promises.Count > 0)
            {
                promiseCopies = new IPromise[promises.Count];
                promises.CopyTo(promiseCopies, 0);

                for (int i = 0; i < promiseCopies.Length; ++i)
                {
                    if (promiseCopies[i] == null)
                    {
                        throw new ArgumentException(null, "promises");
                    }
                }
            }

            if (promise != null)
            {
                while (promise.IsPending)
                {
                    yield return null;
                }
            }

            if (promiseCopies != null)
            {
                for (int i = 0; i < promiseCopies.Length; ++i)
                {
                    while (promiseCopies[i].IsPending)
                    {
                        yield return null;
                    }
                }
            }
        }

        public static IEnumerator GetEnumerator(IPromise promise, IEnumerable<IPromise> promises)
        {
            IEnumerator<IPromise> enumerator = null;

            if (promises != null)
            {
                enumerator = promises.GetEnumerator();
            }

            if (promise != null)
            {
                while (promise.IsPending)
                {
                    yield return null;
                }
            }

            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    if (current == null)
                    {
                        throw new ArgumentException(null, "promises");
                    }

                    while (current.IsPending)
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}
