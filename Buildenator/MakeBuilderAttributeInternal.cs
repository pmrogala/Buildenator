using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;

namespace Buildenator;

internal sealed class MakeBuilderAttributeInternal(
    INamedTypeSymbol typeForBuilder,
    string? buildingMethodsPrefix,
    bool? staticCreator,
    NullableStrategy? nullableStrategy,
    bool? generateMethodsForUnreachableProperties,
    bool? implicitCast,
    string? staticFactoryMethodName,
    bool? generateStaticPropertyForBuilderCreation,
    bool? initializeCollectionsWithEmpty,
    bool? useChildBuilders,
    bool? builderConstructorMandatoryParameters) : System.IEquatable<MakeBuilderAttributeInternal>
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
            (bool?)attribute.ConstructorArguments[9].Value,
            (bool?)attribute.ConstructorArguments[10].Value)
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
    public bool? BuilderConstructorMandatoryParameters { get; } = builderConstructorMandatoryParameters;
    internal string? StaticFactoryMethodName { get; } = staticFactoryMethodName;

    public bool Equals(MakeBuilderAttributeInternal other)
    {
        return
            TypeForBuilder.Equals(other.TypeForBuilder, SymbolEqualityComparer.Default)
            && BuildingMethodsPrefix == other.BuildingMethodsPrefix
            && GenerateDefaultBuildMethod == other.GenerateDefaultBuildMethod
            && ImplicitCast == other.ImplicitCast
            && NullableStrategy == other.NullableStrategy
            && GenerateMethodsForUnreachableProperties == other.GenerateMethodsForUnreachableProperties
            && GenerateStaticPropertyForBuilderCreation == other.GenerateStaticPropertyForBuilderCreation
            && InitializeCollectionsWithEmpty == other.InitializeCollectionsWithEmpty
            && UseChildBuilders == other.UseChildBuilders
            && BuilderConstructorMandatoryParameters == other.BuilderConstructorMandatoryParameters
            && StaticFactoryMethodName == other.StaticFactoryMethodName;
    }

    public override bool Equals(object? obj) => obj is MakeBuilderAttributeInternal other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 31) + SymbolEqualityComparer.Default.GetHashCode(TypeForBuilder);
            hash = (hash * 31) + (BuildingMethodsPrefix?.GetHashCode() ?? 0);
            hash = (hash * 31) + (GenerateDefaultBuildMethod?.GetHashCode() ?? 0);
            hash = (hash * 31) + (ImplicitCast?.GetHashCode() ?? 0);
            hash = (hash * 31) + (NullableStrategy?.GetHashCode() ?? 0);
            hash = (hash * 31) + (GenerateMethodsForUnreachableProperties?.GetHashCode() ?? 0);
            hash = (hash * 31) + (GenerateStaticPropertyForBuilderCreation?.GetHashCode() ?? 0);
            hash = (hash * 31) + (InitializeCollectionsWithEmpty?.GetHashCode() ?? 0);
            hash = (hash * 31) + (UseChildBuilders?.GetHashCode() ?? 0);
            hash = (hash * 31) + (BuilderConstructorMandatoryParameters?.GetHashCode() ?? 0);
            hash = (hash * 31) + (StaticFactoryMethodName?.GetHashCode() ?? 0);
            return hash;
        }
    }
}