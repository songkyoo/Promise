using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_With : PromiseTest
    {
        [UnityTest]
        public IEnumerator PromiseCancelledWhenGameObjectDisabled()
        {
            var go = new GameObject();
            var promise = Promise.Create((resolve, reject) => {}).With(go);

            GameObject.Destroy(go);

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
        }

        [UnityTest]
        public IEnumerator PromiseWillRemoveFromObserverWhenPromiseSettled()
        {
            var go = new GameObject();
            var promise = Promise
                .Create(
                    (resolve, reject) =>
                    {
                        ThreadPool.QueueUserWorkItem(
                            _ =>
                            {
                                Thread.Sleep(100);
                                resolve();
                            });
                    })
                .With(go);
            var observer = go.GetComponent<Observer>();

            Expect(observer, Not.Null);
            Expect(observer, Count.EqualTo(1));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(go.GetComponent<Observer>(), Count.Zero);
        }

        [UnityTest]
        public IEnumerator CalledWithToDestroyedGameObjectWillCancelPromise()
        {
            var go = new GameObject();
            GameObject.Destroy(go);

            var promise = Promise.Create((resolve, reject) => {}).With(go);

            yield return promise;

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
        }

        [UnityTest]
        public IEnumerator CalledWithToInactiveGameObjectWillCancelPromise()
        {
            var go = new GameObject();
            go.SetActive(false);

            var promise = Promise.Create((resolve, reject) => {}).With(go);

            yield return promise;

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
        }

        [UnityTest]
        public IEnumerator MultipleCalledWithToSinglePromiseCancelledWhenGameObjectDisabled()
        {
            var go = new GameObject();
            var promise = Promise.Create((resolve, reject) => {});

            var children = new[]
            {
                promise.With(go),
                promise.With(go),
                promise.With(go)
            };

            Expect(children.Distinct().Count(), EqualTo(children.Length));

            GameObject.Destroy(go);

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
        }

        [UnityTest]
        public IEnumerator MultipleCalledWithToSinglePromiseWillRemoveFromObserverWhenPromiseSettled()
        {
            var go = new GameObject();
            var promise = Promise
                .Create(
                    (resolve, reject) =>
                    {
                        ThreadPool.QueueUserWorkItem(
                            _ =>
                            {
                                Thread.Sleep(100);
                                resolve();
                            });
                    });

            var children = new[]
            {
                promise.With(go),
                promise.With(go),
                promise.With(go)
            };

            Expect(children.Distinct().Count(), EqualTo(children.Length));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(go.GetComponent<Observer>(), Count.Zero);
        }

        [UnityTest]
        public IEnumerator ChainedWithCancelledWhenGameObjectDisabled()
        {
            var go = new GameObject();
            var promise = Promise.Create((resolve, reject) => {}).With(go).With(go).With(go);
            var observer = go.GetComponent<Observer>();

            Expect(observer, Not.Null);
            Expect(observer, Count.EqualTo(3));

            GameObject.Destroy(go);

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
        }

        [UnityTest]
        public IEnumerator ChainedWithWillRemoveFromObserverWhenPromiseSettled()
        {
            var go = new GameObject();
            var promise = Promise
                .Create(
                    (resolve, reject) =>
                    {
                        ThreadPool.QueueUserWorkItem(
                            _ =>
                            {
                                Thread.Sleep(100);
                                resolve();
                            });
                    })
                .With(go)
                .With(go)
                .With(go);
            var observer = go.GetComponent<Observer>();

            Expect(observer, Not.Null);
            Expect(observer, Count.EqualTo(3));

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(go.GetComponent<Observer>(), Count.Zero);
        }
    }
}
