using System;

namespace Macaron
{
    [Serializable]
    public class UnhandledRejectionException : Exception
    {
        public UnhandledRejectionException() : base()
        {
        }

        public UnhandledRejectionException(string message) : base(message)
        {
        }

        public UnhandledRejectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
