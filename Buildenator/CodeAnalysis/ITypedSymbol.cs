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
    /// Gets collection metadata if this symbol represents a collection type (interface or concrete).
    /// Returns null if not a collection.
    /// </summary>
    /// <param name="childBuilderLookup">Optional function to look up child builder name by element type display name.</param>
    CollectionMetadata? GetCollectionMetadata(System.Func<string, string?>? childBuilderLookup);
    
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
    /// <summary>
    /// The short name of the element type (e.g., "ChildForParentEntity").
    /// Used for type comparison purposes.
    /// </summary>
    public string ElementTypeName { get; }
    
    /// <summary>
    /// The full display string of the element type (e.g., "Namespace.ChildForParentEntity").
    /// Used for code generation.
    /// </summary>
    public string ElementTypeDisplayName { get; }
    
    /// <summary>
    /// If the collection's element type has a builder, this contains the builder name.
    /// </summary>
    public string? ChildBuilderName { get; }
    
    /// <summary>
    /// Whether this collection has a child builder for its element type.
    /// </summary>
    public bool HasChildBuilder => ChildBuilderName != null;
    
    protected CollectionMetadata(ITypeSymbol elementType, string? childBuilderName)
    {
        ElementTypeName = elementType.Name;
        ElementTypeDisplayName = elementType.ToDisplayString();
        ChildBuilderName = childBuilderName;
    }
}

/// <summary>
/// Metadata for interface collection types (e.g., IEnumerable<T>, IList<T>).
/// </summary>
internal sealed class InterfaceCollectionMetadata : CollectionMetadata
{
    public InterfaceCollectionMetadata(ITypeSymbol elementType, string? childBuilderName = null) 
        : base(elementType, childBuilderName)
    {
    }
}

/// <summary>
/// Metadata for concrete collection types (e.g., List<T>, HashSet<T>).
/// </summary>
internal sealed class ConcreteCollectionMetadata : CollectionMetadata
{
    public ConcreteCollectionMetadata(ITypeSymbol elementType, string? childBuilderName = null) 
        : base(elementType, childBuilderName)
    {
    }
}

/// <summary>
/// Metadata for concrete dictionary types (e.g., Dictionary<TKey, TValue>).
/// </summary>
internal sealed class ConcreteDictionaryMetadata : CollectionMetadata
{
    public string KeyTypeDisplayName { get; }
    public string ValueTypeDisplayName { get; }
    
    public ConcreteDictionaryMetadata(ITypeSymbol keyType, ITypeSymbol valueType, ITypeSymbol elementType, string? childBuilderName = null) 
        : base(elementType, childBuilderName)
    {
        KeyTypeDisplayName = keyType.ToDisplayString();
        ValueTypeDisplayName = valueType.ToDisplayString();
    }
}

/// <summary>
/// Metadata for interface dictionary types (e.g., IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>).
/// </summary>
internal sealed class InterfaceDictionaryMetadata : CollectionMetadata
{
    public string KeyTypeDisplayName { get; }
    public string ValueTypeDisplayName { get; }
    
    public InterfaceDictionaryMetadata(ITypeSymbol keyType, ITypeSymbol valueType, ITypeSymbol elementType, string? childBuilderName = null) 
        : base(elementType, childBuilderName)
    {
        KeyTypeDisplayName = keyType.ToDisplayString();
        ValueTypeDisplayName = valueType.ToDisplayString();
    }
}