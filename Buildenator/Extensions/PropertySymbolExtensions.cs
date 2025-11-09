using Microsoft.CodeAnalysis;

namespace Buildenator.Extensions;

internal static class PropertySymbolExtensions
{
    public static bool IsSettableProperty(this IPropertySymbol x)
        => x.SetMethod is not null;

    public static bool IsPubliclySettableProperty(this IPropertySymbol x)
        => x.SetMethod is not null && x.SetMethod.DeclaredAccessibility == Accessibility.Public && x.CanBeReferencedByName;
}