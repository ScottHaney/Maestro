using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core
{
    public static class HashSetExtensions
    {
        public static bool AreEquivalent<T>(this HashSet<T> lhs, HashSet<T> rhs)
            where T : IEquatable<T>
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            if (lhs.Count != rhs.Count)
                return false;

            return lhs.All(x => rhs.Contains(x));
        }
    }
}
