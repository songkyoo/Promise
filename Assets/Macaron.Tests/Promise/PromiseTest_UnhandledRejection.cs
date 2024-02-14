using System;
using System.Collections;
using UnityEngine.TestTools;

namespace Macaron.Tests
{
    public class PromiseTest_UnhandledRejection : PromiseTest
    {
        [UnityTest]
        public IEnumerator ThrowUnhandledRejectionOnFinalize()
        {
            var reason = new Exception("Unhandled rejection.");
            var unhandledRejection = default(Exception);

            Promise.SetUnhandledRejectionHandler(
                e =>
                {
                    unhandledRejection = e;
                });

            Promise.Reject(reason);

            yield return null;

            GC.Collect();

            while (unhandledRejection == null)
            {
                yield return null;
            }

            Promise.SetUnhandledRejectionHandler(null);

            Expect(unhandledRejection, EqualTo(reason));
        }
    }
}
