using Microsoft.CodeAnalysis;

namespace Buildenator
{
    internal static class ParameterSymbolExtensions
    {
        public static string PascalCaseName(this IParameterSymbol symbol)
            => $"{symbol.Name.Substring(0, 1).ToUpperInvariant()}{symbol.Name.Substring(1)}";
        public static string UnderScoreName(this IParameterSymbol symbol)
            => $"_{symbol.Name.Substring(0, 1).ToLowerInvariant()}{symbol.Name.Substring(1)}";
    }
}