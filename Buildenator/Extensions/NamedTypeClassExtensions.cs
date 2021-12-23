using Microsoft.CodeAnalysis;

namespace Buildenator.Extensions
{
    internal static class NamedTypeClassExtensions
    {
        internal static bool HasNameOrBaseClassHas(this INamedTypeSymbol? symbol, string name)
            => symbol is not null && (symbol.Name == name || symbol.BaseType.HasNameOrBaseClassHas(name));
    }
}