using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_Resolve : PromiseTest
    {
        [UnityTest]
        public IEnumerator Resolve()
        {
            int value = 765;
            int resolvedValue = 0;
            int frameCount = Time.frameCount;

            var promise = Promise
                .Resolve(value)
                .Then(
                    x =>
                    {
                        resolvedValue = x;
                        return x;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            while (promise.IsPending)
            {
                yield return null;
            }

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
            Expect(resolvedValue, EqualTo(value));
            Expect(Time.frameCount, GreaterThan(frameCount));
        }

        [UnityTest]
        public IEnumerator ResolveMultipleSubscription()
        {
            var value = 765;
            var promise = Promise.Create<int>(
                (resolve, reject) =>
                {
                    resolve(value);
                });
            Func<int, int> identity = x => x;

            var children = new[]
            {
                promise.Then(identity),
                promise.Then(identity),
                promise.Then(identity)
            };

            var grandchildren = new[]
            {
                children[0].Then(identity),
                children[0].Then(identity),
                children[0].Then(identity),

                children[1].Then(identity),
                children[1].Then(identity),
                children[1].Then(identity),

                children[2].Then(identity),
                children[2].Then(identity),
                children[2].Then(identity)
            };

            yield return grandchildren.ToYieldInstruction();

            Expect(grandchildren.Select(p => p.State), All.EqualTo(PromiseState.Fulfilled));
            Expect(grandchildren.Select(p => p.Value), All.EqualTo(value));
        }

        [UnityTest]
        public IEnumerator ResolvePromise()
        {
            int value = 765;
            int resolvedValue = 0;
            int frameCount = Time.frameCount;

            var promise = Promise
                .Resolve(Promise.Resolve(value))
                .Then(
                    x =>
                    {
                        resolvedValue = x;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            while (promise.IsPending)
            {
                yield return null;
            }

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(resolvedValue, EqualTo(value));
            Expect(Time.frameCount, GreaterThan(frameCount));
        }

        [UnityTest]
        public IEnumerator IgnoreResolveAfterFulfilled()
        {
            int valueFirst = 765;
            int valueSecond = 573;
            int resolvedValue = 0;

            var promise = Promise
                .Create<int>(
                    (resolve, reject) =>
                    {
                        resolve(valueFirst);
                        resolve(valueSecond);
                    })
                .Then(
                    value =>
                    {
                        resolvedValue = value;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(resolvedValue, EqualTo(valueFirst));
        }

        [UnityTest]
        public IEnumerator IgnoreResolveAfterRejected()
        {
            var reason = new Exception("Error.");
            int value = 765;
            var rejectedReason = default(Exception);

            var promise = Promise
                .Create<int>(
                    (resolve, reject) =>
                    {
                        reject(reason);
                        resolve(value);
                    })
                .Catch(
                    x =>
                    {
                        rejectedReason = x;
                    });

            Expect(promise.State, EqualTo(PromiseState.Pending));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(rejectedReason, EqualTo(reason));
        }

        [UnityTest]
        public IEnumerator IgnoreResolveAfterCancelled()
        {
            int value = 765;
            int resolvedValue = 0;
            var resolvePromise = default(Action<int>);

            var promise = Promise
                .Create<int>(
                    (resolve, reject) =>
                    {
                        resolvePromise = resolve;
                    })
                .Then(
                    x =>
                    {
                        resolvedValue = x;
                    });

            promise.Cancel();
            resolvePromise(value);

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(resolvedValue, Not.EqualTo(value));
        }
    }
}
