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
    /// Gets collection metadata if this symbol represents a collection type (interface or concrete).
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
    
    /// <summary>
    /// True if this is a concrete collection type (e.g., List<T>, HashSet<T>),
    /// false if this is an interface collection type (e.g., IEnumerable<T>, IList<T>).
    /// </summary>
    public bool IsConcreteType { get; }
    
    public CollectionMetadata(ITypeSymbol elementType, bool isConcreteType)
    {
        ElementType = elementType;
        IsConcreteType = isConcreteType;
    }
}