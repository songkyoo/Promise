using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Macaron
{
    public static class PromiseExtensionMethods
    {
        private static class Stubs
        {
            public static readonly Action Nothing = () => {};
        }

        public static Promise<TResult> Then<TResult>(
            this Promise<Nothing> promise,
            Func<TResult> onFulfilled,
            Func<Exception, TResult> onRejected = null)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (onFulfilled == null)
            {
                throw new ArgumentNullException("onFulfilled");
            }

            return promise.Then(
                _ =>
                {
                    return onFulfilled();
                },
                onRejected);
        }

        public static Promise<TResult> Then<TResult>(
            this Promise<Nothing> promise,
            Func<Promise<TResult>> onFulfilled,
            Func<Exception, Promise<TResult>> onRejected = null)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (onFulfilled == null)
            {
                throw new ArgumentNullException("onFulfilled");
            }

            return promise.Then(
                _ =>
                {
                    return onFulfilled();
                },
                onRejected);
        }

        public static Promise<Nothing> Then(
            this Promise<Nothing> promise,
            Action onFulfilled,
            Action<Exception> onRejected = null)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (onFulfilled == null)
            {
                throw new ArgumentNullException("onFulfilled");
            }

            return promise.Then(
                _ =>
                {
                    onFulfilled();
                },
                onRejected);
        }

        public static Promise<T> Catch<T>(
            this Promise<T> promise,
            Func<Exception, bool> predicate,
            Func<Exception, T> onRejected)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            return promise.Catch(predicate, onRejected);
        }

        public static Promise<T> Catch<T>(this Promise<T> promise, Func<Exception, T> onRejected)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            return promise.Catch(onRejected);
        }

        public static Promise<T> Catch<T>(
            this Promise<T> promise,
            Func<Exception, bool> predicate,
            Func<Exception, Promise<T>> onRejected)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            return promise.Catch(predicate, onRejected);
        }

        public static Promise<T> Catch<T>(this Promise<T> promise, Func<Exception, Promise<T>> onRejected)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            return promise.Catch(onRejected);
        }

        public static Promise<Nothing> Catch<T>(
            this Promise<T> promise,
            Func<Exception, bool> predicate,
            Action<Exception> onRejected)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            return promise.Catch(predicate, onRejected);
        }

        public static Promise<Nothing> Catch<T>(this Promise<T> promise, Action<Exception> onRejected)
        {
            return promise.Catch(onRejected);
        }

        public static Promise<T> SuppressUnhandledRejection<T>(this Promise<T> promise)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            promise.IgnoreUnhandledRejection = true;
            return promise;
        }

        public static Promise<T> On<T>(this Promise<T> promise, IDispatcher dispatcher)
        {
            return Promise<T>.Resolve(dispatcher, promise);
        }

        /// <summary>
        /// 프로미스를 지정한 게임 오브젝트가 비활성화될 때 취소를 요청하도록 연결한다. 메인 스레드에서 호출되어야 한다.
        /// </summary>
        /// <param name="promise">게임 오브젝트를 연결할 프로미스.</param>
        /// <param name="go">비활성화 시 연결된 프로미스에 취소를 요청할 게임 오브젝트.</param>
        /// <returns>
        /// 취소 요청 처리를 추가한 새로운 프로미스를 반환한다. <paramref name="go"/>가 활성 상태가 아닐 경우 취소를 요청한 새로운
        /// 프로미스를 반환한다.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="promise"/> 혹은 <paramref name="go"/>가 <c>null</c> 값인 경우.
        /// </exception>
        public static Promise<T> With<T>(this Promise<T> promise, GameObject go)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (go == null)
            {
                throw new ArgumentNullException("go");
            }

            if (!go.activeInHierarchy)
            {
                Promise<T> finallyPromise = promise.Finally(Stubs.Nothing);
                finallyPromise.Cancel();

                return finallyPromise;
            }

            var observer = go.GetComponent<Observer>();
            if (observer == null)
            {
                observer = go.AddComponent<Observer>();
            }

            if (!observer.isActiveAndEnabled)
            {
                Promise<T> finallyPromise = promise.Finally(Stubs.Nothing);
                finallyPromise.Cancel();

                return finallyPromise;
            }

            return observer.Register(promise);
        }

        /// <summary>
        /// 프로미스를 지정한 컴포넌트가 속한 게임 오브젝트가 비활성화될 때 취소를 요청하도록 연결한다. 메인 스레드에서 호출되어야 한다.
        /// </summary>
        /// <param name="promise">게임 오브젝트를 연결할 프로미스.</param>
        /// <param name="component">비활성화 시 연결된 프로미스에 취소를 요청할 게임 오브젝트에 속한 컴포넌트.</param>
        /// <returns>
        /// 취소 요청 처리를 추가한 새로운 프로미스를 반환한다. <paramref name="component"/>가 속한 게임 오브젝트가 활성 상태가 아닐
        /// 경우 취소를 요청한 새로운 프로미스를 반환한다.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="promise"/> 혹은 <paramref name="component"/>가 <c>null</c> 값인 경우.
        /// </exception>
        public static Promise<T> With<T>(this Promise<T> promise, Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }

            return promise.With(component.gameObject);
        }

        #if UNITY_5_3_OR_NEWER
        /// <summary>
        /// 프로미스의 완료를 기다리는 열거자를 반환한다.
        /// </summary>
        /// <param name="promise">완료를 기다릴 프로미스.</param>
        /// <returns>프로미스의 완료를 기다리는 <see cref="IEnumerator"/> 개체.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promise"/>가 <c>null</c> 값인 경우.</exception>
        public static IEnumerator ToYieldInstruction(this IPromise promise)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            return Internal.PromiseYieldInstruction.GetEnumerator(promise);
        }

        /// <summary>
        /// 프로미스 컬렉션의 완료를 기다리는 열거자를 반환한다.
        /// </summary>
        /// <param name="promises">완료를 기다릴 프로미스 컬렉션.</param>
        /// <returns>프로미스 컬렉션의 완료를 기다리는 <see cref="IEnumerator"/> 개체.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promises"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="promises"/>를 순회 중에 <c>null</c> 값을 반환하는 경우.
        /// </exception>
        /// <remarks>
        /// <paramref name="promises"/>에 담긴 요소를 한 번에 보관하지 않고 열거자를 사용하여 프로미스가 완료된 후 다음 프로미스를
        /// 가져온다.
        /// </remarks>
        public static IEnumerator ToYieldInstruction(this IEnumerable<IPromise> promises)
        {
            if (promises == null)
            {
                throw new ArgumentNullException("promises");
            }

            return Internal.PromiseYieldInstruction.GetEnumerator(null, promises);
        }

        /// <summary>
        /// 프로미스 컬렉션의 완료를 기다리는 열거자를 반환한다.
        /// </summary>
        /// <param name="promises">완료를 기다릴 프로미스 컬렉션.</param>
        /// <returns>프로미스 컬렉션의 완료를 기다리는 <see cref="IEnumerator"/> 개체.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promises"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="promises"/>의 요소 중 <c>null</c> 값이 있는 경우.
        /// </exception>
        public static IEnumerator ToYieldInstruction(this ICollection<IPromise> promises)
        {
            if (promises == null)
            {
                throw new ArgumentNullException("promises");
            }

            return Internal.PromiseYieldInstruction.GetEnumerator(null, promises);
        }

        /// <summary>
        /// 프로미스와 프로미스 컬렉션의 완료를 기다리는 열거자를 반환한다.
        /// </summary>
        /// <param name="promise">완료를 기다릴 프로미스.</param>
        /// <param name="promises">완료를 기다릴 프로미스 컬렉션.</param>
        /// <returns>프로미스와 프로미스 컬렉션의 완료를 기다리는 <see cref="IEnumerator"/> 개체.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promise"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="promises"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="promises"/>를 순회 중에 <c>null</c> 값을 반환하는 경우.
        /// </exception>
        /// <remarks>
        /// <paramref name="promise"/>가 완료된 후 <paramref name="promises"/>를 순회하며, <paramref name="promises"/>에
        /// 담긴 요소를 한 번에 보관하지 않고 열거자를 사용하여 프로미스가 완료된 후 다음 프로미스를 가져온다.
        /// </remarks>
        public static IEnumerator ToYieldInstructionWith(this IPromise promise, IEnumerable<IPromise> promises)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (promises == null)
            {
                throw new ArgumentNullException("promises");
            }

            return Internal.PromiseYieldInstruction.GetEnumerator(promise, promises);
        }

        /// <summary>
        /// 프로미스와 프로미스 컬렉션의 완료를 기다리는 열거자를 반환한다.
        /// </summary>
        /// <param name="promise">완료를 기다릴 프로미스.</param>
        /// <param name="promises">완료를 기다릴 프로미스 컬렉션.</param>
        /// <returns>프로미스와 프로미스 컬렉션의 완료를 기다리는 <see cref="IEnumerator"/> 개체.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promise"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="promises"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="promises"/>의 요소 중 <c>null</c> 값이 있는 경우.
        /// </exception>
        /// <remarks><paramref name="promise"/>가 완료된 후 <paramref name="promises"/>를 순회한다.</remarks>
        public static IEnumerator ToYieldInstructionWith(this IPromise promise, ICollection<IPromise> promises)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (promises == null)
            {
                throw new ArgumentNullException("promises");
            }

            return Internal.PromiseYieldInstruction.GetEnumerator(promise, promises);
        }

        /// <summary>
        /// 프로미스와 프로미스 배열의 완료를 기다리는 열거자를 반환한다.
        /// </summary>
        /// <param name="promise">완료를 기다릴 프로미스.</param>
        /// <param name="promises">완료를 기다릴 프로미스 배열.</param>
        /// <returns>프로미스와 프로미스 배열의 완료를 기다리는 <see cref="IEnumerator"/> 개체.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promise"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="promises"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="promises"/>의 요소 중 <c>null</c> 값이 있는 경우.
        /// </exception>
        /// <remarks><paramref name="promise"/>가 완료된 후 <paramref name="promises"/>를 순회한다.</remarks>
        public static IEnumerator ToYieldInstructionWith(this IPromise promise, params IPromise[] promises)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (promises == null)
            {
                throw new ArgumentNullException("promises");
            }

            return Internal.PromiseYieldInstruction.GetEnumerator(promise, promises);
        }
        #endif
    }
}
