using Microsoft.CodeAnalysis;

namespace Buildenator.CodeAnalysis;

internal interface ITypedSymbol
{
    string SymbolName { get; }
    string SymbolPascalName { get; }
    string TypeFullName { get; }
    string TypeName { get; }
    string UnderScoreName { get; }

    string GenerateFieldInitialization();
    string GenerateFieldType();
    string GenerateFieldValueReturn();
    string GenerateLazyFieldType();
    string GenerateLazyFieldValueReturn();
    string GenerateMethodParameterDefinition();
    bool IsFakeable();
    bool IsMockable();
    bool NeedsFieldInit();
    
    /// <summary>
    /// Gets collection metadata if this symbol represents an interface collection type.
    /// Returns null if not a collection.
    /// </summary>
    CollectionMetadata? GetCollectionMetadata();
}

/// <summary>
/// Metadata about a collection property, encapsulating collection-related information.
/// </summary>
internal sealed class CollectionMetadata
{
    public ITypeSymbol ElementType { get; }
    
    public CollectionMetadata(ITypeSymbol elementType)
    {
        ElementType = elementType;
    }
}