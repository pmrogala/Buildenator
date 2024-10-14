using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Buildenator.Abstraction;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Buildenator.Configuration;

internal sealed class EntityToBuild : IEntityToBuild
{
    public string Name { get; }
    public string FullName { get; }
    public string FullNameWithConstraints { get; }
    public Constructor ConstructorToBuild { get; }
    public IReadOnlyList<TypedSymbol> SettableProperties { get; }
    public IReadOnlyList<TypedSymbol> ReadOnlyProperties { get; }
    public string[] AdditionalNamespaces { get; }

    public EntityToBuild(
        INamedTypeSymbol typeForBuilder,
        IMockingProperties? mockingConfiguration,
        IFixtureProperties? fixtureConfiguration,
        NullableStrategy nullableStrategy)
    {
        INamedTypeSymbol? entityToBuildSymbol;
        var additionalNamespaces = Enumerable.Empty<string>();
        if (typeForBuilder.IsGenericType)
        {
            entityToBuildSymbol = typeForBuilder.ConstructedFrom;
            additionalNamespaces = entityToBuildSymbol.TypeParameters.Where(a => a.ConstraintTypes.Any())
                .SelectMany(a => a.ConstraintTypes).Select(a => a.ContainingNamespace.ToDisplayString())
                .ToArray();
        }
        else
        {
            entityToBuildSymbol = typeForBuilder;
        }

        AdditionalNamespaces = additionalNamespaces.Concat(new[] { entityToBuildSymbol.ContainingNamespace.ToDisplayString() }).ToArray();
        Name = entityToBuildSymbol.Name;
        FullName = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
        FullNameWithConstraints = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance));

        ConstructorToBuild = Constructor.CreateConstructorOrDefault(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration, nullableStrategy);
        (SettableProperties, ReadOnlyProperties) = DividePropertiesBySetability(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration, nullableStrategy);
    }

    public IReadOnlyList<ITypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
    {
        return _uniqueTypedSymbols ??= SettableProperties
            .Where(x => !ConstructorToBuild.ContainsParameter(x.SymbolName))
            .Concat(ConstructorToBuild.Parameters).ToList();
    }

    public IReadOnlyList<ITypedSymbol> GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch()
    {
        return _uniqueReadOnlyTypedSymbols ??= ReadOnlyProperties
            .Where(x => !ConstructorToBuild.ContainsParameter(x.SymbolName)).ToList();
    }

    private static (TypedSymbol[] Settable, TypedSymbol[] ReadOnly) DividePropertiesBySetability(
        INamedTypeSymbol entityToBuildSymbol, IMockingProperties? mockingConfiguration,
        IFixtureProperties? fixtureConfiguration, NullableStrategy nullableStrategy)
    {
        var (settable, readOnly) = entityToBuildSymbol.DividePublicPropertiesBySetability();
        return (
            settable.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration, nullableStrategy)).ToArray(),
            readOnly.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration, nullableStrategy)).ToArray());
    }

    private IReadOnlyList<TypedSymbol>? _uniqueTypedSymbols;
    private IReadOnlyList<TypedSymbol>? _uniqueReadOnlyTypedSymbols;

    internal sealed class Constructor
    {
        public bool IsPrivate { get; }

        public static Constructor CreateConstructorOrDefault(
            INamedTypeSymbol entityToBuildSymbol,
            IMockingProperties? mockingConfiguration,
            IFixtureProperties? fixtureConfiguration,
            NullableStrategy nullableStrategy)
        {
            var constructors = entityToBuildSymbol.Constructors.Select(a => a).ToArray();
            var onlyPublicConstructors = constructors
                .Where(m => m.DeclaredAccessibility == Accessibility.Public || m.DeclaredAccessibility == Accessibility.Internal)
                .ToList();

            var isPrivate = onlyPublicConstructors.Count == 0;

            Dictionary<string, TypedSymbol> parameters = [];
            if (!isPrivate)
            {
                parameters = onlyPublicConstructors
                    .OrderByDescending(x => x.Parameters.Length)
                    .First()
                    .Parameters
                    .ToDictionary(x => x.PascalCaseName(), s => new TypedSymbol(s, mockingConfiguration, fixtureConfiguration, nullableStrategy));
            }

            return new Constructor(parameters, isPrivate);
        }

        private Constructor(IReadOnlyDictionary<string, TypedSymbol> constructorParameters, bool isPrivate)
        {
            ConstructorParameters = constructorParameters;
            IsPrivate = isPrivate;
        }

        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }

        public bool ContainsParameter(string parameterName) => ConstructorParameters.ContainsKey(parameterName);
        public IEnumerable<TypedSymbol> Parameters => ConstructorParameters.Values;
    }
}