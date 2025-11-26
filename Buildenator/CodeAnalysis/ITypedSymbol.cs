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
/// Base class for collection metadata, encapsulating collection-related information.
/// </summary>
internal abstract class CollectionMetadata
{
    public ITypeSymbol ElementType { get; }
    
    protected CollectionMetadata(ITypeSymbol elementType)
    {
        ElementType = elementType;
    }
}

/// <summary>
/// Metadata for interface collection types (e.g., IEnumerable<T>, IList<T>).
/// </summary>
internal sealed class InterfaceCollectionMetadata : CollectionMetadata
{
    public InterfaceCollectionMetadata(ITypeSymbol elementType) : base(elementType)
    {
    }
}

/// <summary>
/// Metadata for concrete collection types (e.g., List<T>, HashSet<T>).
/// </summary>
internal sealed class ConcreteCollectionMetadata : CollectionMetadata
{
    public ConcreteCollectionMetadata(ITypeSymbol elementType) : base(elementType)
    {
    }
}

/// <summary>
/// Metadata for concrete dictionary types (e.g., Dictionary<TKey, TValue>).
/// </summary>
internal sealed class ConcreteDictionaryMetadata : CollectionMetadata
{
    public ITypeSymbol KeyType { get; }
    public ITypeSymbol ValueType { get; }
    
    public ConcreteDictionaryMetadata(ITypeSymbol keyType, ITypeSymbol valueType, ITypeSymbol elementType) 
        : base(elementType)
    {
        KeyType = keyType;
        ValueType = valueType;
    }
}

/// <summary>
/// Metadata for interface dictionary types (e.g., IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>).
/// </summary>
internal sealed class InterfaceDictionaryMetadata : CollectionMetadata
{
    public ITypeSymbol KeyType { get; }
    public ITypeSymbol ValueType { get; }
    
    public InterfaceDictionaryMetadata(ITypeSymbol keyType, ITypeSymbol valueType, ITypeSymbol elementType) 
        : base(elementType)
    {
        KeyType = keyType;
        ValueType = valueType;
    }
}