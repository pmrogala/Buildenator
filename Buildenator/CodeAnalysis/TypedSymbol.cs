using Buildenator.Abstraction;
using Buildenator.Configuration.Contract;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Buildenator.Configuration;

namespace Buildenator.CodeAnalysis;
internal sealed class TypedSymbol : ITypedSymbol
{
    public TypedSymbol(
        IPropertySymbol symbol,
        IMockingProperties? mockingInterfaceStrategy,
        IFixtureProperties? fixtureConfiguration,
        NullableStrategy nullableStrategy,
        IReadOnlyCollection<string>? defaultValueNames = null)
        : this(symbol, symbol.Type, mockingInterfaceStrategy, fixtureConfiguration, nullableStrategy, defaultValueNames)
    {
    }

    public TypedSymbol(
        IParameterSymbol symbol,
        IMockingProperties? mockingInterfaceStrategy,
        IFixtureProperties? fixtureConfiguration,
        NullableStrategy nullableStrategy,
        IReadOnlyCollection<string>? defaultValueNames = null)
        : this(symbol, symbol.Type, mockingInterfaceStrategy, fixtureConfiguration, nullableStrategy, defaultValueNames)
    {
    }

    private TypedSymbol(
        ISymbol symbol,
        ITypeSymbol typeSymbol,
        IMockingProperties? mockingInterfaceStrategy,
        IFixtureProperties? fixtureConfiguration,
        NullableStrategy nullableStrategy,
        IReadOnlyCollection<string>? defaultValueNames)
    {
        Symbol = symbol;
        Type = typeSymbol;
        _mockingProperties = mockingInterfaceStrategy;
        _fixtureProperties = fixtureConfiguration;
        _nullableStrategy = nullableStrategy;
        _defaultValueNames = defaultValueNames;
    }

    public bool NeedsFieldInit() => IsMockable();

    private ISymbol Symbol { get; }
    private ITypeSymbol Type { get; }

    private string? _underscoreName;
    public string UnderScoreName => _underscoreName ??= Symbol.UnderScoreName();

    private string? _typeFullName;
    public string TypeFullName => _typeFullName ??= Type.ToDisplayString();

    public string TypeName => Type.Name;

    public string SymbolPascalName => Symbol.PascalCaseName();
    public string SymbolName => Symbol.Name;

    private CollectionMetadata? _collectionMetadata;
    private bool _collectionMetadataInitialized;
    
    public CollectionMetadata? GetCollectionMetadata()
    {
        if (!_collectionMetadataInitialized)
        {
            _collectionMetadataInitialized = true;
            _collectionMetadata = CollectionMethodDetector.CreateCollectionMetadata(Type);
        }
        return _collectionMetadata;
    }

    private readonly IMockingProperties? _mockingProperties;
    private bool? _isMockable;
    public bool IsMockable()
        => _isMockable ??= _mockingProperties?.Strategy switch
        {
            MockingInterfacesStrategy.All
                when Type.TypeKind == TypeKind.Interface => true,
            MockingInterfacesStrategy.WithoutGenericCollection
                when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => true,
            _ => false
        };


    private readonly IFixtureProperties? _fixtureProperties;
    private readonly NullableStrategy? _nullableStrategy;
    private readonly IReadOnlyCollection<string>? _defaultValueNames;
    private bool? _isFakeable;
    public bool IsFakeable()
        => _isFakeable ??= _fixtureProperties?.Strategy switch
        {
            null => false,
            FixtureInterfacesStrategy.None
                when Type.TypeKind == TypeKind.Interface => false,
            FixtureInterfacesStrategy.OnlyGenericCollections
                when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => false,
            _ => true
        };

    public string GenerateFieldInitialization()
        => _mockingProperties is null ? string.Empty : $"{UnderScoreName} = {string.Format(_mockingProperties.FieldDefaultValueAssignmentFormat, TypeFullName)};";


    public string GenerateFieldType()
    {
        var collectionMetadata = GetCollectionMetadata();
        
        // For mockable types, use the mocking framework's type
        if (IsMockable())
            return GenerateMockableFieldType();
        
        // For concrete dictionary types, use the actual type name
        if (collectionMetadata is ConcreteDictionaryMetadata)
            return TypeFullName;
        
        // For interface dictionary types, use Dictionary<K,V>
        if (collectionMetadata is InterfaceDictionaryMetadata dictMetadata)
            return $"System.Collections.Generic.Dictionary<{dictMetadata.KeyType.ToDisplayString()}, {dictMetadata.ValueType.ToDisplayString()}>";
        
        // For concrete collection types, use the actual type name
        if (collectionMetadata is ConcreteCollectionMetadata)
            return TypeFullName;
        
        // For interface collection types, use List<T>
        if (collectionMetadata is InterfaceCollectionMetadata)
            return $"System.Collections.Generic.List<{collectionMetadata.ElementType.ToDisplayString()}>";
        
        // For all other types, use the type's full name
        return TypeFullName;
    }

    public string GenerateLazyFieldType()
        => IsMockable() ? GenerateMockableFieldType() : $"{DefaultConstants.NullBox}<{TypeFullName}>?";

    public string GenerateLazyFieldValueReturn()
        => IsMockable()
            ? string.Format(_mockingProperties!.ReturnObjectFormat, UnderScoreName)
            : @$"({UnderScoreName}.HasValue ? {UnderScoreName}.Value : new {DefaultConstants.NullBox}<{TypeFullName}>({(IsFakeable()
                ? $"{string.Format(_fixtureProperties!.CreateSingleFormat, TypeFullName, SymbolName, DefaultConstants.FixtureLiteral)}"
                  + (_nullableStrategy == NullableStrategy.Enabled ? "!" : "")
                : $"default({TypeFullName})")})).Object";

    public string GenerateFieldValueReturn()
        => IsMockable()
            ? string.Format(_mockingProperties!.ReturnObjectFormat, UnderScoreName)
            : UnderScoreName;

    public string GenerateMethodParameterDefinition()
        => IsMockable() ? $"Action<{GenerateMockableFieldType()}> {DefaultConstants.SetupActionLiteral}" : $"{TypeFullName} {DefaultConstants.ValueLiteral}";

    public string? GetDefaultValueName()
    {
        if (_defaultValueNames == null)
            return null;
        
        var defaultFieldName = $"{DefaultConstants.DefaultFieldPrefix}{SymbolPascalName}";
        return _defaultValueNames.Contains(defaultFieldName) ? defaultFieldName : null;
    }

    private string GenerateMockableFieldType() => string.Format(_mockingProperties!.TypeDeclarationFormat, TypeFullName);
}