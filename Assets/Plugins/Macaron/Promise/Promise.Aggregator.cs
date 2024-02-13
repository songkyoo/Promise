using System;

namespace Macaron
{
    partial class Promise<T>
    {
        private class Aggregator
        {
            public readonly T[] Values;
            public readonly int Count;
            public int CompletedCount;

            public Aggregator(int count)
            {
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count");
                }

                Values = new T[count];
                Count = count;
            }
        }
    }
}
