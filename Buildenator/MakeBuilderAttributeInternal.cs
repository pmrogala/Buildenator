using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;

namespace Buildenator;

internal readonly struct MakeBuilderAttributeInternal(
    INamedTypeSymbol typeForBuilder,
    string? buildingMethodsPrefix,
    bool? staticCreator,
    NullableStrategy? nullableStrategy,
    bool? generateMethodsForUnreachableProperties,
    bool? implicitCast,
    string? staticFactoryMethodName,
    bool? generateStaticPropertyForBuilderCreation,
    bool? initializeCollectionsWithEmpty,
    bool? useChildBuilders)
{

    public MakeBuilderAttributeInternal(AttributeData attribute)
        : this(
            (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!,
            (string?)attribute.ConstructorArguments[1].Value,
            (bool?)attribute.ConstructorArguments[2].Value,
            attribute.ConstructorArguments[3].Value is null
                ? null
                : (NullableStrategy)attribute.ConstructorArguments[3].Value!,
            (bool?)attribute.ConstructorArguments[4].Value,
            (bool?)attribute.ConstructorArguments[5].Value,
            (string?)attribute.ConstructorArguments[6].Value,
            (bool?)attribute.ConstructorArguments[7].Value,
            (bool?)attribute.ConstructorArguments[8].Value,
            (bool?)attribute.ConstructorArguments[9].Value)
    {

    }

    public INamedTypeSymbol TypeForBuilder { get; } = typeForBuilder;
    public string? BuildingMethodsPrefix { get; } = buildingMethodsPrefix;
    public bool? GenerateDefaultBuildMethod { get; } = staticCreator;
    public bool? ImplicitCast { get; } = implicitCast;
    public NullableStrategy? NullableStrategy { get; } = nullableStrategy;
    public bool? GenerateMethodsForUnreachableProperties { get; } = generateMethodsForUnreachableProperties;
    public bool? GenerateStaticPropertyForBuilderCreation { get; } = generateStaticPropertyForBuilderCreation;
    public bool? InitializeCollectionsWithEmpty { get; } = initializeCollectionsWithEmpty;
    public bool? UseChildBuilders { get; } = useChildBuilders;
    internal string? StaticFactoryMethodName { get; } = staticFactoryMethodName;
}