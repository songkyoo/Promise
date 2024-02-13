using System;
using System.Collections.Generic;
using UnityEngine;
using Macaron.Internal;

namespace Macaron
{
    [DisallowMultipleComponent]
    public class Observer : MonoBehaviour
    {
        private readonly List<ICancelable> _promises = new List<ICancelable>();
        private int _version;

        #region MonoBehaviour Messages
        private void OnDisable()
        {
            _version += 1;

            for (int i = 0; i < _promises.Count; ++i)
            {
                _promises[i].Cancel();
            }

            _promises.Clear();
        }
        #endregion

        /// <summary>
        /// 등록된 프로미스의 수.
        /// </summary>
        public int Count
        {
            get { return _promises.Count; }
        }

        /// <summary>
        /// <see cref="Observer"/> 컴포넌트가 비활성화될 때 취소를 요청할 프로미스를 등록한다.
        /// <see cref="PromiseExtensionMethods.With{T}(Promise{T},GameObject)"/> 메서드에서만 호출되어야 한다.
        /// </summary>
        /// <param name="promise"><see cref="Observer"/> 컴포넌트가 속한 게임 오브젝트에 연결할 프로미스.</param>
        /// <returns>취소 요청 처리를 추가한 새로운 프로미스를 반환한다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promise"/>가 <c>null</c> 값인 경우.</exception>
        /// <exception cref="InvalidOperationException">게임 오브젝트가 비활성 상태인 경우.</exception>
        internal Promise<T> Register<T>(Promise<T> promise)
        {
            if (promise == null)
            {
                throw new ArgumentNullException("promise");
            }

            if (!isActiveAndEnabled)
            {
                throw new InvalidOperationException();
            }

            int version = _version;
            Promise<T> finallyPromise = null;

            finallyPromise = promise.Finally(
                () =>
                {
                    MainThreadDispatcher.Send(
                        _ =>
                        {
                            if (version == _version)
                            {
                                _promises.Remove(finallyPromise);
                            }
                        },
                        null);
                });

            _promises.Add(finallyPromise);

            return finallyPromise;
        }
    }
}
