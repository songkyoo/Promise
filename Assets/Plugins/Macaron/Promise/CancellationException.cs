using System;

namespace Macaron
{
    [Serializable]
    public class CancellationException : Exception
    {
        public CancellationException() : base()
        {
        }

        public CancellationException(string message) : base(message)
        {
        }

        public CancellationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
