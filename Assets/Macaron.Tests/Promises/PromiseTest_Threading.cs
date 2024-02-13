using System;
using System.Collections;
using System.Threading;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_Threading : PromiseTest
    {
        [UnityTest]
        public IEnumerator TreatThreadAbortExceptionAsCancellation()
        {
            Thread thread = null;

            var promise = Promise
                .Create(
                    (resolve, reject) =>
                    {
                        thread = new Thread(
                            state =>
                            {
                                try
                                {
                                    Thread.Sleep(1000);
                                    resolve();
                                }
                                catch (Exception e)
                                {
                                    reject(e);
                                }
                            });

                        thread.Start();
                    })
                .SuppressUnhandledRejection();

            yield return null;

            thread.Abort();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Rejected));
            Expect(promise.Reason, TypeOf<ThreadAbortException>());
        }
    }
}
