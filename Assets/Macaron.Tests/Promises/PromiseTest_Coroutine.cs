using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_Coroutine : PromiseTest
    {
        [UnityTest]
        public IEnumerator ResolvedWithValue()
        {
            float delay = 1.0f;
            int value = 765;

            var promise = Promise.CreateFromCoroutine<int>(
                resolver =>
                {
                    return ResolvedWithValueCoroutine(delay, resolver, value);
                });

            float startTime = Time.time;

            yield return promise.ToYieldInstruction();

            Expect(Time.time, GreaterThan(startTime + delay));
            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator RejectedWithReason()
        {
            float delay = 1.0f;
            var reason = new Exception("Error.");

            var promise = Promise
                .CreateFromCoroutine<int>(
                    resolver =>
                    {
                        return RejectedWithReasonCoroutine(delay, resolver, reason);
                    })
                .SuppressUnhandledRejection();

            float startTime = Time.time;

            yield return promise.ToYieldInstruction();

            Expect(Time.time, GreaterThan(startTime + delay));
            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reason));
        }

        [UnityTest]
        public IEnumerator ResolvedWhenCoroutineFinished()
        {
            float delay = 1.0f;

            var promise = Promise.CreateFromCoroutine(
                () =>
                {
                    return ResolvedWhenCoroutineFinishedCoroutine(delay);
                });

            float startTime = Time.time;

            yield return promise.ToYieldInstruction();

            Expect(Time.time, GreaterThan(startTime + delay));
            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
        }

        [UnityTest]
        public IEnumerator RejectedWhenCoroutineThrowException()
        {
            float delay = 1.0f;
            var reason = new Exception("Error.");

            var promise = Promise
                .CreateFromCoroutine(
                    () =>
                    {
                        return RejectedWhenCoroutineThrowExceptionCoroutine(delay, reason);
                    })
                .SuppressUnhandledRejection();

            float startTime = Time.time;

            yield return promise.ToYieldInstruction();

            Expect(Time.time, GreaterThan(startTime + delay));
            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, EqualTo(reason));
        }

        private static IEnumerator ResolvedWithValueCoroutine<T>(float delay, Resolver<T> resolver, T value)
        {
            yield return new WaitForSeconds(delay);

            resolver.Resolve(value);
        }

        private static IEnumerator RejectedWithReasonCoroutine<T>(float delay, Resolver<T> resolver, Exception reason)
        {
            yield return new WaitForSeconds(delay);

            resolver.Reject(reason);
        }

        private static IEnumerator ResolvedWhenCoroutineFinishedCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
        }

        private static IEnumerator RejectedWhenCoroutineThrowExceptionCoroutine(float delay, Exception reason)
        {
            yield return new WaitForSeconds(delay);

            throw reason;
        }
    }
}
