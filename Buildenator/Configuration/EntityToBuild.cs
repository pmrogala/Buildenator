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
        public string Name { get; }
        public string FullName { get; }
        public string FullNameWithConstraints { get; }
        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }
        public IEnumerable<TypedSymbol> SettableProperties { get; }
        public IEnumerable<TypedSymbol> ReadOnlyProperties { get; }
        public string[] AdditionalNamespaces { get; }

        public EntityToBuild(INamedTypeSymbol typeForBuilder, IMockingProperties? mockingConfiguration, IFixtureProperties? fixtureConfiguration)
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
            _mockingConfiguration = mockingConfiguration;
            _fixtureConfiguration = fixtureConfiguration;
            ConstructorParameters = GetConstructorParameters(entityToBuildSymbol);
            (SettableProperties, ReadOnlyProperties) = DividePropertiesBySetability(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration);
        }

        public IReadOnlyList<ITypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
        {
            return _uniqueTypedSymbols ??= SettableProperties
                .Where(x => !ConstructorParameters.ContainsKey(x.SymbolName))
                .Concat(ConstructorParameters.Values).ToList();
        }

        public IReadOnlyList<ITypedSymbol> GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch()
        {
            return _uniqueReadOnlyTypedSymbols ??= ReadOnlyProperties
                .Where(x => !ConstructorParameters.ContainsKey(x.SymbolName)).ToList();
        }

        private IReadOnlyDictionary<string, TypedSymbol> GetConstructorParameters(INamedTypeSymbol entityToBuildSymbol)
        {
            return entityToBuildSymbol.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters
                .ToDictionary(x => x.PascalCaseName(), s => new TypedSymbol(s, _mockingConfiguration, _fixtureConfiguration));
        }

        private static (TypedSymbol[] Settable, TypedSymbol[] ReadOnly) DividePropertiesBySetability(
            INamedTypeSymbol entityToBuildSymbol, IMockingProperties? mockingConfiguration, IFixtureProperties? fixtureConfiguration)
        {
	        var (settable, readOnly) = entityToBuildSymbol.DividePublicPropertiesBySetability();
	        return (
                settable.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration)).ToArray(),
                readOnly.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration)).ToArray());
        }

        private IReadOnlyList<TypedSymbol>? _uniqueTypedSymbols;
        private IReadOnlyList<TypedSymbol>? _uniqueReadOnlyTypedSymbols;
        private readonly IMockingProperties? _mockingConfiguration;
        private readonly IFixtureProperties? _fixtureConfiguration;
    }
}