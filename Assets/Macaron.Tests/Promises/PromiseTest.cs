using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace Macaron.Tests
{
    public class PromiseTest : AssertionHelper, IPrebuildSetup
    {
        #region Static
        public static void TypeInference<T>(Action<T> action)
        {
        }

        public static IEnumerator Delay<T>(float seconds, Action<T> action, T value)
        {
            yield return new WaitForSeconds(seconds);

            action(value);
        }

        public static IEnumerator Delay(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);

            action();
        }
        #endregion

        #region Implementations of IPrebuildSetup
        public void Setup()
        {
            MainThreadDispatcher.Initialize();
        }
        #endregion
    }
}
