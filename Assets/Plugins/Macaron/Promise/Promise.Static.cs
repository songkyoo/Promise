using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Macaron
{
    partial class Promise<T>
    {
        private static class Stubs
        {
            public static readonly Action<object> Cancel = state =>
            {
                var promise = (Promise<T>)state;
                bool hasPublishers = promise.Unbind();

                if (!hasPublishers)
                {
                    promise.Apply(Asynchronously);
                }
            };
            public static readonly Action<object> Publish = state =>
            {
                var subscription = (Internal.ISubscription)state;
                subscription.Publish();
            };
            public static readonly Action<object> Apply = state =>
            {
                var promise = (Promise<T>)state;
                promise.Apply();
            };
        }

        private static class Stubs<TPublisher>
        {
            public static readonly Action<object> Handle = state =>
            {
                var subscription = (Internal.Subscription<TPublisher, T>)state;
                subscription.Subscriber.Handle(subscription);
            };
        }

        public static Promise<T> Resolve(IDispatcher dispatcher, T value)
        {
            return new Promise<T>(dispatcher, PromiseState.Fulfilled, value, null);
        }

        public static Promise<T> Resolve(IDispatcher dispatcher, Promise<T> promise)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            if (promise == null)
            {
                return Resolve(dispatcher, default(T));
            }

            if (dispatcher == promise._dispatcher)
            {
                return promise;
            }

            var subscriber = new Promise<T>(dispatcher);
            subscriber.Resolve(promise);

            return subscriber;
        }

        public static Promise<T> Reject(IDispatcher dispatcher, Exception reason)
        {
            return new Promise<T>(dispatcher, PromiseState.Rejected, default(T), reason);
        }

        public static Promise<T[]> All(IDispatcher dispatcher, IEnumerable<Promise<T>> promises)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            var publishers = new List<Promise<T>>(promises);
            int count = publishers.Count;

            if (count == 0)
            {
                return Promise<T[]>.Resolve(dispatcher, new T[0]);
            }

            var subscriber = new Promise<T[]>(dispatcher);
            var aggregator = new Aggregator(count);

            for (int i = 0; i < count; ++i)
            {
                publishers[i].Register(subscriber, new SubscriptionProvider_All(aggregator, i));
            }

            return subscriber;
        }

        public static Promise<T> Race(IDispatcher dispatcher, IEnumerable<Promise<T>> promises)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            var publishers = new List<Promise<T>>(promises);
            int count = publishers.Count;

            if (count == 0)
            {
                throw new ArgumentException(null, "promises");
            }

            var subscriber = new Promise<T>(dispatcher);

            for (int i = 0; i < count; ++i)
            {
                publishers[i].Register(subscriber, new SubscriptionProvider_Any());
            }

            return subscriber;
        }
    }

    public static partial class Promise
    {
        private static IDispatcher _defaultDispatcher = Internal.MainThreadDispatcher.Instance;
        private static Action<Exception> _unhandledRejectionHandler;

        public static void SetDefaultDispatcher(IDispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            _defaultDispatcher = dispatcher;
        }

        public static void SetUnhandledRejectionHandler(Action<Exception> handler)
        {
            _unhandledRejectionHandler = handler;
        }

        internal static void HandleUnhandledRejection(Exception e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (_unhandledRejectionHandler == null)
            {
                throw new UnhandledRejectionException(null, e);
            }

            _unhandledRejectionHandler.Invoke(e);
        }

        public static Promise<T> Resolve<T>(IDispatcher dispatcher, T value)
        {
            return Promise<T>.Resolve(dispatcher, value);
        }

        public static Promise<T> Resolve<T>(T value)
        {
            return Resolve(_defaultDispatcher, value);
        }

        public static Promise<T> Resolve<T>(IDispatcher dispatcher, Promise<T> promise)
        {
            return Promise<T>.Resolve(dispatcher, promise);
        }

        public static Promise<T> Resolve<T>(Promise<T> promise)
        {
            return Resolve(_defaultDispatcher, promise);
        }

        public static Promise<Nothing> Resolve(IDispatcher dispatcher)
        {
            return Resolve(dispatcher, Nothing.Value);
        }

        public static Promise<Nothing> Resolve()
        {
            return Resolve(_defaultDispatcher);
        }

        public static Promise<T> Reject<T>(IDispatcher dispatcher, Exception reason)
        {
            return Promise<T>.Reject(dispatcher, reason);
        }

        public static Promise<T> Reject<T>(Exception reason)
        {
            return Reject<T>(_defaultDispatcher, reason);
        }

        public static Promise<Nothing> Reject(IDispatcher dispatcher, Exception reason)
        {
            return Promise<Nothing>.Reject(dispatcher, reason);
        }

        public static Promise<Nothing> Reject(Exception reason)
        {
            return Reject(_defaultDispatcher, reason);
        }

        public static Promise<T[]> All<T>(IDispatcher dispatcher, IEnumerable<Promise<T>> promises)
        {
            return Promise<T>.All(dispatcher, promises);
        }

        public static Promise<T[]> All<T>(IEnumerable<Promise<T>> promises)
        {
            return All(_defaultDispatcher, promises);
        }

        public static Promise<T[]> All<T>(IDispatcher dispatcher, params Promise<T>[] promises)
        {
            return All(dispatcher, (IEnumerable<Promise<T>>)promises);
        }

        public static Promise<T[]> All<T>(params Promise<T>[] promises)
        {
            return All(_defaultDispatcher, promises);
        }

        public static Promise<T> Race<T>(IDispatcher dispatcher, IEnumerable<Promise<T>> promises)
        {
            return Promise<T>.Race(dispatcher, promises);
        }

        public static Promise<T> Race<T>(IEnumerable<Promise<T>> promises)
        {
            return Race(_defaultDispatcher, promises);
        }

        public static Promise<T> Race<T>(IDispatcher dispatcher, params Promise<T>[] promises)
        {
            return Race(dispatcher, (IEnumerable<Promise<T>>)promises);
        }

        public static Promise<T> Race<T>(params Promise<T>[] promises)
        {
            return Race(_defaultDispatcher, promises);
        }

        public static Promise<T> Create<T>(IDispatcher dispatcher, Func<Action<T>, Action<Exception>, Action> executor)
        {
            return new Promise<T>(dispatcher, executor);
        }

        public static Promise<T> Create<T>(Func<Action<T>, Action<Exception>, Action> executor)
        {
            return Create(_defaultDispatcher, executor);
        }

        public static Promise<T> Create<T>(
            IDispatcher dispatcher,
            Func<Action<Promise<T>>, Action<Exception>, Action> executor)
        {
            return new Promise<T>(dispatcher, executor);
        }

        public static Promise<T> Create<T>(Action<Action<T>, Action<Exception>> executor)
        {
            return Create(_defaultDispatcher, executor);
        }

        public static Promise<T> Create<T>(
            IDispatcher dispatcher,
            Action<Action<Promise<T>>, Action<Exception>> executor)
        {
            return new Promise<T>(dispatcher, executor);
        }

        public static Promise<T> Create<T>(Func<Action<Promise<T>>, Action<Exception>, Action> executor)
        {
            return Create(_defaultDispatcher, executor);
        }

        public static Promise<T> Create<T>(IDispatcher dispatcher, Action<Action<T>, Action<Exception>> executor)
        {
            return new Promise<T>(dispatcher, executor);
        }

        public static Promise<T> Create<T>(Action<Action<Promise<T>>, Action<Exception>> executor)
        {
            return Create(_defaultDispatcher, executor);
        }

        public static Promise<Nothing> Create(IDispatcher dispatcher, Func<Action, Action<Exception>, Action> executor)
        {
            return new Promise<Nothing>(
                dispatcher,
                (resolve, reject) =>
                {
                    return executor(
                        () =>
                        {
                            resolve(Nothing.Value);
                        },
                        reject);
                });
        }

        public static Promise<Nothing> Create(Func<Action, Action<Exception>, Action> executor)
        {
            return Create(_defaultDispatcher, executor);
        }

        public static Promise<Nothing> Create(IDispatcher dispatcher, Action<Action, Action<Exception>> executor)
        {
            return new Promise<Nothing>(
                dispatcher,
                (resolve, reject) =>
                {
                    executor(
                        () =>
                        {
                            resolve(Nothing.Value);
                        },
                        reject);
                });
        }

        public static Promise<Nothing> Create(Action<Action, Action<Exception>> executor)
        {
            return Create(_defaultDispatcher, executor);
        }

        public static Promise<T> CreateFromCoroutine<T>(
            IDispatcher dispatcher,
            Func<Resolver<T>, IEnumerator> coroutine)
        {
            if (coroutine == null)
            {
                throw new ArgumentNullException("coroutine");
            }

            Promise<T> promise = null;
            Action<T> resolve = null;
            Action<Exception> reject = null;

            promise = new Promise<T>(
                dispatcher,
                (res, rej) =>
                {
                    resolve = res;
                    reject = rej;
                });

            var resolver = new Resolver<T>(promise, resolve, reject);
            IEnumerator routine = coroutine(resolver);

            MainThreadDispatcher.SendStartCoroutine(routine);

            return promise;
        }

        public static Promise<T> CreateFromCoroutine<T>(Func<Resolver<T>, IEnumerator> coroutine)
        {
            return CreateFromCoroutine(_defaultDispatcher, coroutine);
        }

        public static Promise<Nothing> CreateFromCoroutine(IDispatcher dispatcher, Func<IEnumerator> coroutine)
        {
            if (coroutine == null)
            {
                throw new ArgumentNullException("coroutine");
            }

            Promise<Nothing> promise = null;
            Action<Nothing> resolve = null;
            Action<Exception> reject = null;

            promise = new Promise<Nothing>(
                dispatcher,
                (res, rej) =>
                {
                    resolve = res;
                    reject = rej;
                });

            var resolver = new Resolver<Nothing>(promise, resolve, reject);
            IEnumerator routine = coroutine();
            IEnumerator adapter = WrapCoroutine(routine, resolver);

            MainThreadDispatcher.SendStartCoroutine(adapter);

            return promise;
        }

        public static Promise<Nothing> CreateFromCoroutine(Func<IEnumerator> coroutine)
        {
            return CreateFromCoroutine(_defaultDispatcher, coroutine);
        }

        public static Promise<T> CreateFromAsyncOperation<T>(
            IDispatcher dispatcher,
            T asyncOperation,
            Action<float> progress)
            where T : AsyncOperation
        {
            Promise<T> promise = null;
            Action<T> resolve = null;
            Action<Exception> reject = null;

            promise = new Promise<T>(
                dispatcher,
                (res, rej) =>
                {
                    resolve = res;
                    reject = rej;
                });

            var resolver = new Resolver<T>(promise, resolve, reject);
            IEnumerator routine = WrapAsyncOperation(asyncOperation, resolver, progress);

            MainThreadDispatcher.SendStartCoroutine(routine);

            return promise;
        }

        public static Promise<T> CreateFromAsyncOperation<T>(T asyncOperation, Action<float> progress)
            where T : AsyncOperation
        {
            return CreateFromAsyncOperation(_defaultDispatcher, asyncOperation, progress);
        }

        public static Promise<T> CreateFromAsyncOperation<T>(IDispatcher dispatcher, T asyncOperation)
            where T : AsyncOperation
        {
            Promise<T> promise = null;
            Action<T> resolve = null;
            Action<Exception> reject = null;

            promise = new Promise<T>(
                dispatcher,
                (res, rej) =>
                {
                    resolve = res;
                    reject = rej;
                });

            var resolver = new Resolver<T>(promise, resolve, reject);
            IEnumerator routine = WrapAsyncOperation(asyncOperation, resolver);

            MainThreadDispatcher.SendStartCoroutine(routine);

            return promise;
        }

        public static Promise<T> CreateFromAsyncOperation<T>(T asyncOperation)
            where T : AsyncOperation
        {
            return CreateFromAsyncOperation(_defaultDispatcher, asyncOperation);
        }

        private static IEnumerator WrapCoroutine(IEnumerator enumerator, Resolver<Nothing> resolver)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException("enumerator");
            }

            while (resolver.IsPending)
            {
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    resolver.Reject(e);
                    yield break;
                }

                yield return enumerator.Current;
            }

            if (resolver.IsSettled)
            {
                yield break;
            }

            resolver.Resolve(Nothing.Value);
        }

        private static IEnumerator WrapAsyncOperation<T>(T asyncOperation, Resolver<T> resolver, Action<float> progress)
            where T : AsyncOperation
        {
            while (!asyncOperation.isDone && resolver.IsPending)
            {
                try
                {
                    progress.Invoke(asyncOperation.progress);
                }
                catch (Exception e)
                {
                    resolver.Reject(e);
                    yield break;
                }

                yield return null;
            }

            if (resolver.IsSettled)
            {
                yield break;
            }

            try
            {
                progress.Invoke(asyncOperation.progress);
            }
            catch (Exception e)
            {
                resolver.Reject(e);
                yield break;
            }

            resolver.Resolve(asyncOperation);
        }

        private static IEnumerator WrapAsyncOperation<T>(T asyncOperation, Resolver<T> resolver)
            where T : AsyncOperation
        {
            if (!asyncOperation.isDone)
            {
                yield return asyncOperation;
            }

            if (resolver.IsSettled)
            {
                yield break;
            }

            resolver.Resolve(asyncOperation);
        }
    }
}
