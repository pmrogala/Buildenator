using System.Collections.Generic;

namespace Buildenator.Extensions
{
    internal static class StringExtensions
    {
        public static string ComaJoin(this IEnumerable<string> strings) => string.Join(", ", strings);
    }

}