using Microsoft.CodeAnalysis;

namespace Buildenator.CodeAnalysis;

internal static class SymbolDisplayFormats
{
    internal static readonly SymbolDisplayFormat TypeWithNamespaceAndGenerics = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    internal static readonly SymbolDisplayFormat TypeWithGenerics = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    internal static readonly SymbolDisplayFormat TypeWithGenericsAndConstraints = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
                     | SymbolDisplayGenericsOptions.IncludeTypeConstraints
                     | SymbolDisplayGenericsOptions.IncludeVariance,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly);
}
