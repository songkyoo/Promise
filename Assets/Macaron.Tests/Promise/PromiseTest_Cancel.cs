using System;
using System.Collections;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_Cancel : PromiseTest
    {
        [UnityTest]
        public IEnumerator Cancel()
        {
            int onCancelCalledCount = 0;
            int finallyCalledCount = 0;
            bool thenCalled = false;

            Func<Action<Nothing>, Action<Exception>, Action> executor = (resolve, reject) =>
            {
                return () =>
                {
                    onCancelCalledCount += 1;
                };
            };

            var promise = Promise.Create(executor);
            var cancel = promise
                .Then(
                    () =>
                    {
                        thenCalled = true;
                    },
                    reason =>
                    {
                        thenCalled = true;
                    })
                .Finally(
                    () =>
                    {
                        finallyCalledCount += 1;
                    });

            yield return null;

            cancel.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(thenCalled, False);
            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(cancel.State, EqualTo(PromiseState.Cancelled));
            Expect(onCancelCalledCount, EqualTo(1));
            Expect(finallyCalledCount, EqualTo(1));
        }

        [UnityTest]
        public IEnumerator RejectWithCancellationExceptionAfterCancelled()
        {
            Action<Action<Nothing>, Action<Exception>> executor = (resolve, reject) => {};

            var promise = Promise.Create(executor);
            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));

            var rejectedPromise = promise
                .Then(() => {}, _ => {})
                .SuppressUnhandledRejection();

            yield return rejectedPromise.ToYieldInstruction();

            Expect(rejectedPromise.State, EqualTo(PromiseState.Rejected));
            Expect(rejectedPromise.Reason, TypeOf<CancellationException>());
        }

        [UnityTest]
        public IEnumerator CancelMultipleSubscriptionDownward()
        {
            int onCancelCalledCount = 0;
            int onFinallyCalledCount = 0;

            Func<Action<Nothing>, Action<Exception>, Action> executor = (resolve, reject) =>
            {
                return () =>
                {
                    onCancelCalledCount += 1;
                };
            };
            Action onFinally = () =>
            {
                onFinallyCalledCount += 1;
            };

            var promise = Promise.Create(executor);
            var first = promise.Finally(onFinally);
            var second = promise.Finally(onFinally);

            yield return null;

            promise.Cancel();

            Expect(promise.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstructionWith(first, second);

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(first.State, EqualTo(PromiseState.Cancelled));
            Expect(second.State, EqualTo(PromiseState.Cancelled));
            Expect(onCancelCalledCount, EqualTo(1));
            Expect(onFinallyCalledCount, EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CancelMultipleSubscriptionUpward()
        {
            int onCancelCalledCount = 0;
            int onFinallyCalledCount = 0;

            Func<Action<Nothing>, Action<Exception>, Action> executor = (resolve, reject) =>
            {
                return () =>
                {
                    onCancelCalledCount += 1;
                };
            };
            Action onFinally = () =>
            {
                onFinallyCalledCount += 1;
            };

            var promise = Promise.Create(executor);
            var first = promise.Finally(onFinally);
            var second = promise.Finally(onFinally);

            yield return null;

            first.Cancel();
            second.Cancel();

            Expect(first.State, EqualTo(PromiseState.Pending));
            Expect(second.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstructionWith(first, second);

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(first.State, EqualTo(PromiseState.Cancelled));
            Expect(second.State, EqualTo(PromiseState.Cancelled));
            Expect(onCancelCalledCount, EqualTo(1));
            Expect(onFinallyCalledCount, EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CancelPromiseCreatedByOnFulfilledDownward()
        {
            int onCancelCalledCount = 0;
            int onFinallyCalledCount = 0;

            Action onCancel = () =>
            {
                onCancelCalledCount += 1;
            };
            Action onFinally = () =>
            {
                onFinallyCalledCount += 1;
            };

            var innerPromise = default(Promise<Nothing>);
            var promise = Promise
                .Resolve()
                .Then(
                    () =>
                    {
                        Func<Action<Nothing>, Action<Exception>, Action> executor = (resolve, reject) =>
                        {
                            return onCancel;
                        };

                        innerPromise = Promise.Create(executor);
                        return innerPromise.Finally(onFinally);
                    })
                .Finally(onFinally);

            yield return null;

            innerPromise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(onCancelCalledCount, EqualTo(1));
            Expect(onFinallyCalledCount, EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CancelPromiseCreatedByOnFulfilledUpward()
        {
            int onCancelCalledCount = 0;
            int onFinallyCalledCount = 0;

            Action onCancel = () =>
            {
                onCancelCalledCount += 1;
            };
            Action onFinally = () =>
            {
                onFinallyCalledCount += 1;
            };

            var promise = Promise
                .Resolve()
                .Then(
                    () =>
                    {
                        Func<Action<Nothing>, Action<Exception>, Action> executor = (resolve, reject) =>
                        {
                            return onCancel;
                        };

                        return Promise.Create(executor).Finally(onFinally);
                    })
                .Finally(onFinally);

            yield return null;

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(onCancelCalledCount, EqualTo(1));
            Expect(onFinallyCalledCount, EqualTo(2));
        }

        [UnityTest]
        public IEnumerator IgnoreCancelAfterFulfilled()
        {
            int value = 765;
            int resolvedValue = 0;

            var promise = Promise.Create<int>(
                (resolve, reject) =>
                {
                    resolve(value);
                });
            var cancel = promise.Then(
                x =>
                {
                    resolvedValue = x;
                });

            cancel.Cancel();

            yield return cancel.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
            Expect(cancel.State, EqualTo(PromiseState.Cancelled));
            Expect(resolvedValue, Not.EqualTo(value));
        }

        [UnityTest]
        public IEnumerator IgnoreCancelAfterRejected()
        {
            var reason = new Exception("Error.");
            var rejectedReason = default(Exception);

            Action<Action<Nothing>, Action<Exception>> executor = (resolve, reject) =>
            {
                reject(reason);
            };

            var promise = Promise.Create(executor);
            var cancel = promise
                .Catch(
                    x =>
                    {
                        rejectedReason = x;
                    })
                .SuppressUnhandledRejection();

            cancel.Cancel();

            yield return cancel.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reason));
            Expect(cancel.State, EqualTo(PromiseState.Fulfilled));
            Expect(rejectedReason, EqualTo(reason));
        }

        [UnityTest]
        public IEnumerator IgnoreCancelAfterCancelled()
        {
            int onCancelCalledCount = 0;

            Func<Action<Nothing>, Action<Exception>, Action> executor = (resolve, reject) =>
            {
                return () =>
                {
                    onCancelCalledCount += 1;
                };
            };

            var promise = Promise.Create(executor);

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(onCancelCalledCount, EqualTo(1));

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(onCancelCalledCount, EqualTo(1));
        }
    }
}
