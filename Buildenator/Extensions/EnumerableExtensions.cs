using System;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator.Extensions;

internal static class EnumerableExtensions
{
    public static (IEnumerable<T> Left, IEnumerable<T> Right) Split<T>(this IEnumerable<T> source, Func<T, bool> predicate)
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

			return (left.AsEnumerable(), right.AsEnumerable());
		}

    public static (List<T> Left, List<T> Right) ToLists<T>(this (IEnumerable<T> Left, IEnumerable<T> Right) source)
        => (source.Left.ToList(), source.Right.ToList());
}