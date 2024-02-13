using System;

namespace Macaron.Internal
{
    internal static class Predicates<T>
    {
        public static readonly Func<T, bool> True = x =>
        {
            return true;
        };
    }
}
