using System;
using System.Collections;
using UnityEngine;

namespace Macaron
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        #region Static
        private static MainThreadDispatcher _instance;
        [ThreadStatic]
        private static bool _isMainThread;

        private static bool _isQuitting;
        private static Action<Exception> _unhandledExceptionHandler;

        private static readonly Action<object> _startCoroutine = state =>
        {
            StartCoroutine((IEnumerator)state);
        };

        private static MainThreadDispatcher Instance
        {
            get
            {
                Initialize();
                return _instance;
            }
        }

        /// <summary>
        /// MainThreadDispatcher를 초기화한다. 이미 초기화된 MainThreadDispatcher가 존재하거나 프로그램이 종료 중인 경우 아무 것도
        /// 하지 않는다.
        /// </summary>
        public static void Initialize()
        {
            if (_instance != null)
            {
                return;
            }

            // 씬 초기화 중 MainThreadDispatcher 컴포넌트를 포함하는 게임 오브젝트가 씬에 있지만 Awake가 아직 호출되지 않은 상황에서
            // 다른 컴포넌트의 Awake가 MainThreadDispatcher에 접근할 수 있기 때문에 다음과 같은 순서로 초기화를 수행한다.

            MainThreadDispatcher[] dispatchers;

            try
            {
                // 작업 스레드에서 접근할 경우 예외가 발생한다.
                dispatchers = FindObjectsOfType<MainThreadDispatcher>();
            }
            catch
            {
                // 발생한 예외는 로그에 오류로 출력된다. 로그 이벤트 처리를 위해 예외 로그를 출력한다.
                const string msg =
                    "MainThreadDispatcher는 메인 스레드에서만 초기화될 수 있습니다. 작업 스레드에서 접근하기 전에 메인 스레드에서 " +
                    "초기화 작업이 수행되어야 합니다.";
                Debug.LogException(new InvalidOperationException(msg));

                throw;
            }

            if (_isQuitting)
            {
                return;
            }

            MainThreadDispatcher dispatcher = null;

            for (int i = 0; i < dispatchers.Length; ++i)
            {
                // 게임 오브젝트는 활성 상태지만 컴포넌트는 비활성 상태일 수 있음.
                if (dispatchers[i].enabled)
                {
                    dispatcher = dispatchers[i];
                    break;
                }
            }

            if (dispatcher == null)
            {
                // 게임 오브젝트 생성 시 Awake가 호출된다.
                new GameObject("MainThreadDispatcher", typeof(MainThreadDispatcher));
            }
            else
            {
                // 아직 Awake가 호출되지 않았을 수 있기 때문에 _instance 할당을 위해서 강제로 호출한다.
                dispatcher.Awake();
            }
        }

        public static void SetUnhandledExceptionHandler(Action<Exception> handler)
        {
            _unhandledExceptionHandler = handler;
        }

        public static void Post(Action<object> action, object state)
        {
            MainThreadDispatcher instance = Instance;
            if (instance != null)
            {
                instance.Enqueue(action, state);
            }
        }

        public static void Send(Action<object> action, object state)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (_isMainThread)
            {
                try
                {
                    action(state);
                }
                catch (Exception e)
                {
                    if (_unhandledExceptionHandler == null)
                    {
                        throw;
                    }

                    _unhandledExceptionHandler.Invoke(e);
                }
            }
            else
            {
                Post(action, state);
            }
        }

        public static void SendStartCoroutine(IEnumerator routine)
        {
            if (_isMainThread)
            {
                StartCoroutine(routine);
            }
            else
            {
                Post(_startCoroutine, routine);
            }
        }

        new public static Coroutine StartCoroutine(IEnumerator routine)
        {
            MainThreadDispatcher instance = Instance;
            return instance != null ? (instance as MonoBehaviour).StartCoroutine(routine) : null;
        }
        #endregion

        #region Fields
        private readonly Internal.ThreadSafeQueueWorker _queueWorker = new Internal.ThreadSafeQueueWorker();
        #endregion

        #region MonoBehaviour Messages
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                _isMainThread = true;

                DontDestroyOnLoad();
            }
            else if (this != _instance) // 이미 호출된 경우에도 Initialize에서 다시 호출될 수 있기 때문에 비교 필요.
            {
                Debug.LogWarning("초기화된 MainThreadDispatcher가 이미 존재합니다.", this);
            }
        }

        private void Update()
        {
            _queueWorker.Run(_unhandledExceptionHandler);
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == null || this != _instance)
            {
                return;
            }

            _instance = null;
        }
        #endregion

        #region Methods
        private void DontDestroyOnLoad()
        {
            #if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                return;
            }
            #endif

            DontDestroyOnLoad(gameObject);
        }

        private void Enqueue(Action<object> action, object state)
        {
            _queueWorker.Enqueue(action, state);
        }
        #endregion
    }
}
