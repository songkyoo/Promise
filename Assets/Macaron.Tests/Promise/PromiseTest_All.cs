using System;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace Macaron.Tests
{
    public class PromiseTest_All : PromiseTest
    {
        [UnityTest]
        public IEnumerator AllResolvedAsynchronouslyWhenResolvedPromisesPassed()
        {
            var values = new[] { 765, 876, 346 };

            var promise = Promise.All(
                Promise.Resolve(values[0]),
                Promise.Resolve(values[1]),
                Promise.Resolve(values[2]));

            Expect(promise.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(values));
        }

        [UnityTest]
        public IEnumerator AllRejectedAsynchronouslyWhenRejectedPromisesPassed()
        {
            var values = new[] { 765, 346 };
            var reason = new Exception("Error.");

            var promise = Promise
                .All(
                    Promise.Resolve(values[0]),
                    Promise.Resolve(values[1]),
                    Promise.Reject<int>(reason))
                .SuppressUnhandledRejection();

            Expect(promise.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reason));
        }

        [Test]
        public void AllResolvedSynchronouslyWhenEmptyArrayPassed()
        {
            var promise = Promise.All(new Promise<int>[0]);

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, Empty);
        }

        [UnityTest]
        public IEnumerator AllRejectedWithFirstRejectedReason()
        {
            var reasons = new[] { new Exception("First."), new Exception("Second.") };

            var first = Promise
                .Create<int>(
                    (resolve, reject) =>
                    {
                        TypeInference<int>(resolve);
                        MainThreadDispatcher.StartCoroutine(Delay(0.2f, reject, reasons[0]));
                    })
                .SuppressUnhandledRejection();
            var second = Promise
                .Create<int>(
                    (resolve, reject) =>
                    {
                        TypeInference<int>(resolve);
                        MainThreadDispatcher.StartCoroutine(Delay(0.1f, reject, reasons[1]));
                    })
                .SuppressUnhandledRejection();
            var third = Promise
                .Create<int>(
                    (resolve, reject) =>
                    {
                        MainThreadDispatcher.StartCoroutine(Delay(0.3f, resolve, 0));
                    });

            var promise = Promise.All(first, second, third).SuppressUnhandledRejection();

            yield return promise.ToYieldInstructionWith(first, second, third);

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reasons[1]));
        }

        [UnityTest]
        public IEnumerator AllCancelledWithFirstCancelledPromise()
        {
            int onFinallyCalledCount = 0;
            Action<Action, Action<Exception>> executor = (resolve, reject) => {};
            Action onFinally = () =>
            {
                onFinallyCalledCount += 1;
            };

            var first = Promise.Resolve().Finally(onFinally);
            var second = Promise.Create(executor).Finally(onFinally);
            var third = Promise.Create(executor).Finally(onFinally);

            // promise가 취소될 때 third도 취소된다.
            MainThreadDispatcher.StartCoroutine(Delay(0.1f, second.Cancel));

            var promise = Promise.All(first, second, third);

            yield return promise.ToYieldInstructionWith(first, second, third);

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(first.State, EqualTo(PromiseState.Fulfilled));
            Expect(second.State, EqualTo(PromiseState.Cancelled));
            Expect(third.State, EqualTo(PromiseState.Cancelled));
            Expect(onFinallyCalledCount, EqualTo(3));
        }

        [UnityTest]
        public IEnumerator CancelAllCancelInnerPromises()
        {
            var first = Promise.Create<int>((resolve, reject) => { TypeInference<int>(resolve); });
            var second = Promise.Create<int>((resolve, reject) => { TypeInference<int>(resolve); });
            var third = Promise.Resolve(765);
            var promise = Promise.All(first, second, third);

            Expect(first.State, EqualTo(PromiseState.Pending));
            Expect(second.State, EqualTo(PromiseState.Pending));
            Expect(third.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.State, EqualTo(PromiseState.Pending));

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(first.State, EqualTo(PromiseState.Cancelled));
            Expect(second.State, EqualTo(PromiseState.Cancelled));
            Expect(third.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.State, EqualTo(PromiseState.Cancelled));
        }
    }
}
