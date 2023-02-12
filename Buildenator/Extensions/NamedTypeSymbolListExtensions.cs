using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Buildenator.Extensions;

public static class NamedTypeSymbolListExtensions
{
    internal static void MakeDeterministicOrderByName<T>(this List<(INamedTypeSymbol Builder, T)> result) =>
        result.Sort((x, y) =>
        {
            var nameCompare = string.CompareOrdinal(x.Builder.Name, y.Builder.Name);
            return nameCompare != 0
                ? nameCompare
                : string.CompareOrdinal(x.Builder.ContainingNamespace.Name, y.Builder.ContainingNamespace.Name);
        });
}