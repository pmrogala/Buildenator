using System.Collections.Generic;
using System.Linq;

namespace Buildenator
{
    public static class NullReferenceExtensions
    {
        public static IEnumerable<T> IsNotNull<T>(this IEnumerable<T> enumerable)
            where T : notnull => enumerable.Where(x => x != null);

        public static IEnumerable<(T1, T2)> AreNotNull<T1, T2>(this IEnumerable<(T1?, T2?)> enumerable)
            where T1 : notnull
            where T2 : notnull
            => enumerable.Where(x => x.Item1 != null && x.Item2 != null).OfType<(T1, T2)>();
    }
}