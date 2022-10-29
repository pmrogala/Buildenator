using System;
using System.Collections.Generic;

namespace Buildenator.Extensions
{
    internal static class EnumerableExtensions
    {
        public static (IEnumerable<T>, IEnumerable<T>) Split<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var left = new List<T>();
            var right = new List<T>();

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    left.Add(item);
                }
                else
                {
                    right.Add(item);
                }
            }

            return (left, right);
        }
    }

}