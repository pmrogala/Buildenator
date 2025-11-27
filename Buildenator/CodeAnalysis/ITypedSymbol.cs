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
    
    /// <summary>
    /// Gets the user-defined default value name for this symbol, if one exists.
    /// Looks for static fields or constants named "Default{PropertyPascalName}" in the builder class.
    /// </summary>
    /// <returns>The default value name (e.g., "DefaultName") if found, otherwise null.</returns>
    string? GetDefaultValueName();
}

/// <summary>
/// Base class for collection metadata, encapsulating collection-related information.
/// </summary>
internal abstract class CollectionMetadata
{
    public ITypeSymbol ElementType { get; }
    
    /// <summary>
    /// The display string of the element type (eagerly loaded for performance).
    /// </summary>
    public string ElementTypeName { get; }
    
    /// <summary>
    /// If the collection's element type has a builder, this contains the builder name.
    /// Set via <see cref="SetChildBuilderInfo"/> when child builders are discovered.
    /// </summary>
    public string? ChildBuilderName { get; private set; }
    
    /// <summary>
    /// Whether this collection has a child builder for its element type.
    /// </summary>
    public bool HasChildBuilder => ChildBuilderName != null;
    
    protected CollectionMetadata(ITypeSymbol elementType)
    {
        ElementType = elementType;
        ElementTypeName = elementType.ToDisplayString();
    }
    
    /// <summary>
    /// Sets the child builder information for this collection.
    /// </summary>
    /// <param name="childBuilderName">The name of the child builder for the element type.</param>
    public void SetChildBuilderInfo(string childBuilderName)
    {
        ChildBuilderName = childBuilderName;
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