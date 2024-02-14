using System;
using System.Collections;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_Finally : PromiseTest
    {
        [UnityTest]
        public IEnumerator FinallyCalledWhenSubscribeFulfilledPromise()
        {
            int onFinallyCalledCount = 0;

            var promise = Promise
                .Resolve()
                .Finally(
                    () =>
                    {
                        onFinallyCalledCount += 1;
                    });

            yield return promise.ToYieldInstruction();

            Expect(onFinallyCalledCount, EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FinallyCalledWhenSubscribeRejectedPromise()
        {
            int onFinallyCalledCount = 0;
            var reason = new Exception("Error.");

            var promise = Promise
                .Reject(reason)
                .Finally(
                    () =>
                    {
                        onFinallyCalledCount += 1;
                    })
                .SuppressUnhandledRejection();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reason));
            Expect(onFinallyCalledCount, EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FinallyCalledWhenSubscribeCancelledPromise()
        {
            int onFinallyCalledCount = 0;

            Action<Action<Nothing>, Action<Exception>> executor = (resolve, reject) => {};

            var promise = Promise.Create(executor);
            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));

            promise = promise.Finally(
                () =>
                {
                    onFinallyCalledCount += 1;
                });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(onFinallyCalledCount, EqualTo(1));
        }
    }
}
