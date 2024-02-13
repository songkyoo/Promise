using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_Reject : PromiseTest
    {
        [UnityTest]
        public IEnumerator Reject()
        {
            var reason = new Exception("Rejected.");
            var rejectedReason = default(Exception);
            bool settled = false;
            int frameCount = Time.frameCount;

            var promise = Promise
                .Reject(reason)
                .Catch(
                    x =>
                    {
                        rejectedReason = x;
                    })
                .Finally(
                    () =>
                    {
                        settled = true;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            while (!settled)
            {
                yield return null;
            }

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(rejectedReason, EqualTo(reason));
            Expect(Time.frameCount, GreaterThan(frameCount));
        }

        [UnityTest]
        public IEnumerator RejectMultipleSubscription()
        {
            var reason = new Exception("Error.");
            var promise = Promise.Create(
                (resolve, reject) =>
                {
                    reject(reason);
                });
            Action nothing = () => {};

            var children = new[]
            {
                promise.Then(nothing),
                promise.Then(nothing),
                promise.Then(nothing)
            };

            var grandchildren = new[]
            {
                children[0].Then(nothing),
                children[0].Then(nothing),
                children[0].Then(nothing),

                children[1].Then(nothing),
                children[1].Then(nothing),
                children[1].Then(nothing),

                children[2].Then(nothing),
                children[2].Then(nothing),
                children[2].Then(nothing)
            };

            grandchildren = grandchildren.Select(PromiseExtensionMethods.SuppressUnhandledRejection).ToArray();

            yield return grandchildren.ToYieldInstruction();

            Expect(grandchildren.Select(p => p.State), All.EqualTo(PromiseState.Rejected));
            Expect(grandchildren.Select(p => p.Reason), All.EqualTo(reason));
        }

        [UnityTest]
        public IEnumerator RejectPromise()
        {
            var reason = new Exception("Rejected.");
            var rejectedReason = default(Exception);
            bool settled = false;
            int frameCount = Time.frameCount;

            var promise = Promise
                .Resolve(Promise.Reject(reason))
                .Catch(
                    x =>
                    {
                        rejectedReason = x;
                    })
                .Finally(
                    () =>
                    {
                        settled = true;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            while (!settled)
            {
                yield return null;
            }

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(rejectedReason, EqualTo(reason));
            Expect(Time.frameCount, GreaterThan(frameCount));
        }

        [UnityTest]
        public IEnumerator RejectOnFulfilledFailed()
        {
            var reason = new Exception("Rejected.");
            bool settled = false;

            var promise = Promise
                .Resolve()
                .Then(
                    () =>
                    {
                        throw reason;
                    })
                .Finally(
                    () =>
                    {
                        settled = true;
                    })
                .SuppressUnhandledRejection();

            while (!settled)
            {
                yield return null;
            }

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reason));
        }

        [UnityTest]
        public IEnumerator RejectOnRejectedFailedWithSameReason()
        {
            var reason = new Exception("Rejected.");
            bool settled = false;

            var promise = Promise
                .Reject(reason)
                .Then(
                    () => {},
                    x =>
                    {
                        throw x;
                    })
                .Finally(
                    () =>
                    {
                        settled = true;
                    })
                .SuppressUnhandledRejection();

            while (!settled)
            {
                yield return null;
            }

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reason));
        }

        [UnityTest]
        public IEnumerator RejectOnRejectedFailedWithDifferentReason()
        {
            var first = new Exception("First.");
            var second = new Exception("Second.");
            bool settled = false;

            var promise = Promise
                .Reject(first)
                .Then(
                    () => {},
                    x =>
                    {
                        Expect(x, EqualTo(first));

                        throw second;
                    })
                .Finally(
                    () =>
                    {
                        settled = true;
                    })
                .SuppressUnhandledRejection();

            while (!settled)
            {
                yield return null;
            }

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, TypeOf<AggregateException>());
            Expect(promise.Reason.InnerException, EqualTo(first));

            var ae = promise.Reason as AggregateException;

            Expect(ae.InnerExceptions, Count.EqualTo(2));
            Expect(ae.InnerExceptions[0], EqualTo(first));
            Expect(ae.InnerExceptions[1], EqualTo(second));
        }

        [UnityTest]
        public IEnumerator RejectOnCancelFailed()
        {
            var error = new Exception("Error.");
            int finallyCalledCount = 0;

            Func<Action<Nothing>, Action<Exception>, Action> executor = (resolve, reject) =>
            {
                return () =>
                {
                    throw error;
                };
            };

            var promise = Promise
                .Create(executor)
                .Finally(
                    () =>
                    {
                        finallyCalledCount += 1;
                    })
                .SuppressUnhandledRejection();

            yield return null;

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(error));
            Expect(finallyCalledCount, EqualTo(1));
        }

        [UnityTest]
        public IEnumerator RejectOnFinallyFailed()
        {
            var error = new Exception("Error.");
            var rejectedReason = default(Exception);
            bool settled = false;

            Promise
                .Resolve()
                .Finally(
                    () =>
                    {
                        throw error;
                    })
                .Catch(
                    reason =>
                    {
                        rejectedReason = reason;
                    })
                .Finally(
                    () =>
                    {
                        settled = true;
                    });

            while (!settled)
            {
                yield return null;
            }

            Expect(rejectedReason, EqualTo(error));
        }

        [UnityTest]
        public IEnumerator IgnoreRejectAfterFulfilled()
        {
            int value = 765;
            var reason = new Exception("Error.");
            int resolvedValue = 0;

            var promise = Promise
                .Create<int>(
                    (resolve, reject) =>
                    {
                        resolve(value);
                        reject(reason);
                    })
                .Then(
                    x =>
                    {
                        resolvedValue = x;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(resolvedValue, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator IgnoreRejectAfterRejected()
        {
            var reasons = new[] { new Exception("First."), new Exception("Second.") };
            var rejectedReason = default(Exception);

            Action<Action<Nothing>, Action<Exception>> executor = (resolve, reject) =>
            {
                reject(reasons[0]);
                reject(reasons[1]);
            };

            var promise = Promise
                .Create(executor)
                .Catch(
                    x =>
                    {
                        rejectedReason = x;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(rejectedReason, EqualTo(reasons[0]));
        }

        [UnityTest]
        public IEnumerator IgnoreRejectAfterCancelled()
        {
            var reason = new Exception("Error.");
            var rejectedReason = default(Exception);

            Action<Exception> rejectPromise = null;
            Action<Action<Nothing>, Action<Exception>> executor = (resolve, reject) =>
            {
                rejectPromise = reject;
            };

            var promise = Promise
                .Create(executor)
                .Catch(
                    x =>
                    {
                        rejectedReason = x;
                    });

            promise.Cancel();
            rejectPromise(reason);

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(rejectedReason, Not.EqualTo(reason));
        }
    }
}
