using System;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace Macaron.Tests
{
    public class PromiseTest_Catch : PromiseTest
    {
        [UnityTest]
        public IEnumerator CatchHandler()
        {
            int value = 765;
            var reason = new Exception("Error.");

            var promise = Promise
                .Reject<int>(reason)
                .Catch(
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        return value;
                    })
                .Catch(
                    x =>
                    {
                        Assert.Fail();
                        return 0;
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator CatchHandlerWithType()
        {
            int value = 573;
            var reason = new ArgumentException("Error.");

            Action<Exception> rethrow = e =>
            {
                throw e;
            };

            var promise = Promise
                .Reject<int>(reason)
                .Catch<ArgumentNullException>(
                    x =>
                    {
                        rethrow(x);
                        return 0;
                    })
                .Catch<InvalidOperationException>(
                    x =>
                    {
                        Assert.Fail();
                        return 0;
                    })
                .Catch(
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        return value;
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator CatchHandlerWithPredicate()
        {
            int value = 573;
            string paramName = "Foo";
            string paramNameNotUsed = "Bar";
            var reason = new ArgumentException("Error.", paramName);

            var promise = Promise
                .Reject<int>(reason)
                .Catch<ArgumentException>(
                    ae =>
                    {
                        return ae.ParamName == paramNameNotUsed;
                    },
                    x =>
                    {
                        Assert.Fail();
                        return 0;
                    })
                .Catch<Exception>(
                    e =>
                    {
                        var ae = e as ArgumentException;
                        return ae != null && ae.ParamName == paramName;
                    },
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        return value;
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator CatchPromiseHandler()
        {
            int value = 765;
            var reason = new Exception("Error.");

            var promise = Promise
                .Reject<int>(reason)
                .Catch(
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        return Promise.Resolve(value);
                    })
                .Catch(
                    x =>
                    {
                        Assert.Fail();
                        return 0;
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator CatchPromiseHandlerWithType()
        {
            int value = 573;
            var reason = new ArgumentException("Error.");

            Action<Exception> rethrow = e =>
            {
                throw e;
            };

            var promise = Promise
                .Reject<int>(reason)
                .Catch<ArgumentNullException>(
                    x =>
                    {
                        rethrow(x);
                        return Promise.Resolve(0);
                    })
                .Catch<InvalidOperationException>(
                    x =>
                    {
                        Assert.Fail();
                        return 0;
                    })
                .Catch(
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        return Promise.Resolve(value);
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator CatchPromiseHandlerWithPredicate()
        {
            int value = 573;
            string paramName = "Foo";
            string paramNameNotUsed = "Bar";
            var reason = new ArgumentException("Error.", paramName);

            var promise = Promise
                .Reject<int>(reason)
                .Catch<ArgumentException>(
                    ae =>
                    {
                        return ae.ParamName == paramNameNotUsed;
                    },
                    x =>
                    {
                        Assert.Fail();
                        return 0;
                    })
                .Catch<Exception>(
                    e =>
                    {
                        var ae = e as ArgumentException;
                        return ae != null && ae.ParamName == paramName;
                    },
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        return Promise.Resolve(value);
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(promise.Value, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator CatchNoReturnHandler()
        {
            var reason = new Exception("Error.");
            bool catchCalled = false;

            var promise = Promise
                .Reject<int>(reason)
                .Catch(
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        catchCalled = true;
                    })
                .Catch(
                    x =>
                    {
                        Assert.Fail();
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(catchCalled, True);
        }

        [UnityTest]
        public IEnumerator CatchNoReturnHandlerWithType()
        {
            var reason = new ArgumentException("Error.");
            bool catchCalled = false;

            Action<Exception> rethrow = e =>
            {
                throw e;
            };

            var promise = Promise
                .Reject<int>(reason)
                .Catch<ArgumentNullException>(
                    x =>
                    {
                        rethrow(x);
                    })
                .Catch<InvalidOperationException>(
                    x =>
                    {
                        Assert.Fail();
                    })
                .Catch(
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        catchCalled = true;
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(catchCalled, True);
        }

        [UnityTest]
        public IEnumerator CatchNoReturnHandlerWithPredicate()
        {
            string paramName = "Foo";
            string paramNameNotUsed = "Bar";
            var reason = new ArgumentException("Error.", paramName);
            bool catchCalled = false;

            var promise = Promise
                .Reject<int>(reason)
                .Catch<ArgumentException>(
                    ae =>
                    {
                        return ae.ParamName == paramNameNotUsed;
                    },
                    x =>
                    {
                        Assert.Fail();
                    })
                .Catch<Exception>(
                    e =>
                    {
                        var ae = e as ArgumentException;
                        return ae != null && ae.ParamName == paramName;
                    },
                    x =>
                    {
                        Expect(x, EqualTo(reason));
                        catchCalled = true;
                    });

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Fulfilled));
            Expect(catchCalled, True);
        }

        [UnityTest]
        public IEnumerator FulfilledValuePassThroughCatchHandler()
        {
            int value = 765;
            int resolvedValue = 0;

            yield return Promise
                .Resolve(value)
                .Catch(
                    reason =>
                    {
                        return 0;
                    })
                .Then(
                    x =>
                    {
                        resolvedValue = x;
                    })
                .ToYieldInstruction();

            Expect(resolvedValue, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator FulfilledValuePassThroughCatchPromiseHandler()
        {
            int value = 765;
            int resolvedValue = 0;

            yield return Promise
                .Resolve(value)
                .Catch(
                    reason =>
                    {
                        return Promise.Resolve(0);
                    })
                .Then(
                    x =>
                    {
                        resolvedValue = x;
                    })
                .ToYieldInstruction();

            Expect(resolvedValue, EqualTo(value));
        }

        [UnityTest]
        public IEnumerator FulfilledValueCantPassThroughCatchNoReturnHandler()
        {
            int value = 765;
            bool fulfilled = false;

            yield return Promise
                .Resolve(value)
                .Catch(
                    reason =>
                    {
                    })
                .Then(
                    x =>
                    {
                        Expect(x, TypeOf<Nothing>());
                        fulfilled = true;
                    })
                .ToYieldInstruction();

            Expect(fulfilled, True);
        }

        [UnityTest]
        public IEnumerator CatchHandlerCalledWhenCancelAfterRejected()
        {
            bool catchCalled = false;
            bool finallyCalled = false;

            var promise = Promise
                .Reject<int>(new Exception("Error."))
                .Catch(
                    reason =>
                    {
                        catchCalled = true;
                        return 0;
                    })
                .Finally(
                    () =>
                    {
                        finallyCalled = true;
                    })
                .SuppressUnhandledRejection();

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(catchCalled, True);
            Expect(finallyCalled, True);
        }

        [UnityTest]
        public IEnumerator CatchPromiseHandlerCalledWhenCancelAfterRejected()
        {
            bool catchCalled = false;
            bool finallyCalled = false;

            var promise = Promise
                .Reject<int>(new Exception("Error."))
                .Catch(
                    reason =>
                    {
                        catchCalled = true;
                        return Promise.Resolve(0);
                    })
                .Finally(
                    () =>
                    {
                        finallyCalled = true;
                    })
                .SuppressUnhandledRejection();

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(catchCalled, True);
            Expect(finallyCalled, True);
        }

        [UnityTest]
        public IEnumerator CatchNoReturnHandlerCalledWhenCancelAfterRejected()
        {
            bool catchCalled = false;
            bool finallyCalled = false;

            var promise = Promise
                .Reject<int>(new Exception("Error."))
                .Catch(
                    reason =>
                    {
                        catchCalled = true;
                    })
                .Finally(
                    () =>
                    {
                        finallyCalled = true;
                    })
                .SuppressUnhandledRejection();

            promise.Cancel();

            yield return promise.ToYieldInstruction();

            Expect(promise.State, EqualTo(PromiseState.Cancelled));
            Expect(catchCalled, True);
            Expect(finallyCalled, True);
        }
    }
}
