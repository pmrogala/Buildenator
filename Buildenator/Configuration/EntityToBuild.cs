using Buildenator.Abstraction;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator.Configuration
{
    internal sealed class EntityToBuild : IEntityToBuild
    {
        public string ContainingNamespace { get; }
        public string Name { get; }
        public string FullName { get; }
        public string FullNameWithConstraints { get; }
        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }
        public IEnumerable<TypedSymbol> SettableProperties { get; }
        public IEnumerable<TypedSymbol> UnsettableProperties { get; }
        public string[] AdditionalNamespaces { get; }

        public EntityToBuild(INamedTypeSymbol typeForBuilder, MockingProperties? mockingConfiguration, FixtureProperties? fixtureConfiguration)
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
            ContainingNamespace = entityToBuildSymbol.ContainingNamespace.ToDisplayString();
            AdditionalNamespaces = additionalNamespaces.Concat(new[] { ContainingNamespace }).ToArray();
            Name = entityToBuildSymbol.Name;
            FullName = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
            FullNameWithConstraints = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance));
            _mockingConfiguration = mockingConfiguration;
            _fixtureConfiguration = fixtureConfiguration;
            ConstructorParameters = GetConstructorParameters(entityToBuildSymbol);
            (SettableProperties, UnsettableProperties) = DividePropertiesBySetability(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration?.Strategy);
        }

        public IReadOnlyList<TypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
        {
            return _uniqueTypedSymbols ??= SettableProperties
                .Where(x => !ConstructorParameters.ContainsKey(x.SymbolName))
                .Concat(ConstructorParameters.Values).ToList();
        }

        public IReadOnlyList<TypedSymbol> GetAllUniqueNotSettablePropertiesWithoutConstructorsParametersMatch()
        {
            return _uniqueUnsettableTypedSymbols ??= UnsettableProperties
                .Where(x => !ConstructorParameters.ContainsKey(x.SymbolName)).ToList();
        }

        private IReadOnlyDictionary<string, TypedSymbol> GetConstructorParameters(INamedTypeSymbol entityToBuildSymbol)
        {
            return entityToBuildSymbol.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters
                .ToDictionary(x => x.PascalCaseName(), s => new TypedSymbol(s, _mockingConfiguration, _fixtureConfiguration?.Strategy));
        }

        private static (TypedSymbol[] Settable, TypedSymbol[] NotSettable) DividePropertiesBySetability(
            INamedTypeSymbol entityToBuildSymbol, IMockingProperties? mockingConfiguration, FixtureInterfacesStrategy? fixtureConfiguration)
        {
            var properties = entityToBuildSymbol.DividePublicPropertiesBySetability();
            return (
                properties.Settable.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration)).ToArray(),
                properties.NotSettable.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration)).ToArray());
        }

        private IReadOnlyList<TypedSymbol>? _uniqueTypedSymbols;
        private IReadOnlyList<TypedSymbol>? _uniqueUnsettableTypedSymbols;
        private readonly MockingProperties? _mockingConfiguration;
        private readonly FixtureProperties? _fixtureConfiguration;
    }
}