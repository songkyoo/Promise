using System;

namespace Macaron
{
    public struct Nothing : IEquatable<Nothing>
    {
        public static readonly Nothing Value = new Nothing();

        #region Operators
        public static bool operator ==(Nothing x, Nothing y)
        {
            return true;
        }

        public static bool operator !=(Nothing x, Nothing y)
        {
            return false;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            return obj is Nothing;
        }

        public override int GetHashCode()
        {
            return 0;
        }
        #endregion

        #region Implementations of IEquatable<Nothing>
        public bool Equals(Nothing other)
        {
            return true;
        }
        #endregion
    }
}
