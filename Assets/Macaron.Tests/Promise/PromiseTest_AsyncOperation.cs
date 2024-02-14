using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_AsyncOperation : PromiseTest
    {
        [UnityTest]
        public IEnumerator LoadResourceAsync()
        {
            var promise = Promise.CreateFromAsyncOperation(Resources.LoadAsync<TextAsset>("Dummy"));

            yield return promise.ToYieldInstruction();

            var asset = promise.Value.asset as TextAsset;

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(asset, Not.Null);
            Expect(asset.bytes, Length.EqualTo(1024 * 1024));
        }
    }
}
