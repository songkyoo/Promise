using System;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace Macaron.Tests
{
    public class PromiseTest_Race : PromiseTest
    {
        [UnityTest]
        public IEnumerator RaceFulfilledWithFirstFulfilledPromise()
        {
            var values = new[] { 765, 876, 346 };

            var first = Promise.Create<int>(
                (resolve, reject) =>
                {
                    MainThreadDispatcher.StartCoroutine(Delay(0.2f, resolve, values[0]));
                });
            var second = Promise.Create<int>(
                (resolve, reject) =>
                {
                    MainThreadDispatcher.StartCoroutine(Delay(0.1f, resolve, values[1]));
                });
            var third = Promise.Create<int>(
                (resolve, reject) =>
                {
                    MainThreadDispatcher.StartCoroutine(Delay(0.3f, resolve, values[2]));
                });

            var promise = Promise.Race(first, second, third);

            yield return promise.ToYieldInstructionWith(first, second, third);

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(second.Value));
        }

        [UnityTest]
        public IEnumerator RaceRejectedWithFirstRejectedPromise()
        {
            var reasons = new[] { new Exception("First."), new Exception("Second."), new Exception("Third.") };

            var first = Promise.Create(
                (resolve, reject) =>
                {
                    MainThreadDispatcher.StartCoroutine(Delay(0.2f, reject, reasons[0]));
                })
                .SuppressUnhandledRejection();
            var second = Promise.Create(
                (resolve, reject) =>
                {
                    MainThreadDispatcher.StartCoroutine(Delay(0.3f, reject, reasons[1]));
                })
                .SuppressUnhandledRejection();
            var third = Promise.Create(
                (resolve, reject) =>
                {
                    MainThreadDispatcher.StartCoroutine(Delay(0.1f, reject, reasons[2]));
                })
                .SuppressUnhandledRejection();

            var promise = Promise.Race(first, second, third).SuppressUnhandledRejection();

            yield return promise.ToYieldInstructionWith(first, second, third);

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(third.Reason));
        }

        [UnityTest]
        public IEnumerator RaceCancelledWithFirstCancelledPromise()
        {
            int onFinallyCalledCount = 0;
            Action<Action, Action<Exception>> executor = (resolve, reject) => {};
            Action onFinally = () =>
            {
                onFinallyCalledCount += 1;
            };

            var first = Promise.Create(executor).Finally(onFinally);
            var second = Promise.Create(executor).Finally(onFinally);
            var third = Promise.Create(executor).Finally(onFinally);

            // promise가 취소될 때 first와 third도 취소된다.
            MainThreadDispatcher.StartCoroutine(Delay(0.1f, second.Cancel));

            var promise = Promise.Race(first, second, third);

            yield return promise.ToYieldInstructionWith(first, second, third);

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(first.State, EqualTo(PromiseState.Cancelled));
            Expect(second.State, EqualTo(PromiseState.Cancelled));
            Expect(third.State, EqualTo(PromiseState.Cancelled));
            Expect(onFinallyCalledCount, EqualTo(3));
        }

        [UnityTest]
        public IEnumerator CancelRaceCancelInnerPromises()
        {
            int onFinallyCalledCount = 0;
            Action<Action, Action<Exception>> executor = (resolve, reject) => {};
            Action onFinally = () =>
            {
                onFinallyCalledCount += 1;
            };

            var first = Promise.Create(executor).Finally(onFinally);
            var second = Promise.Create(executor).Finally(onFinally);
            var third = Promise.Create(executor).Finally(onFinally);

            var promise = Promise.Race(first, second, third);

            promise.Cancel();

            yield return promise.ToYieldInstructionWith(first, second, third);

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(first.State, EqualTo(PromiseState.Cancelled));
            Expect(second.State, EqualTo(PromiseState.Cancelled));
            Expect(third.State, EqualTo(PromiseState.Cancelled));
            Expect(onFinallyCalledCount, EqualTo(3));
        }

        [Test]
        public void ThrowArgumentExceptionWhenCallRaceWithEmptyCollection()
        {
            try
            {
                Promise.Race(new Promise<Nothing>[0]);
            }
            catch (Exception e)
            {
                Expect(e, TypeOf<ArgumentException>());
                return;
            }

            Assert.Fail();
        }
    }
}
