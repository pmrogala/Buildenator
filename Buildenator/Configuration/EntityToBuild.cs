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
        private IEnumerable<TypedSymbol>? _uniqueTypedSymbols;
        private readonly MockingProperties? _mockingConfiguration;
        private readonly FixtureProperties? _fixtureConfiguration;

        public string ContainingNamespace { get; }
        public string Name { get; }
        public string FullName { get; }
        public string FullNameWithConstraints { get; }
        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }
        public IEnumerable<TypedSymbol> SettableProperties { get; }
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
            AdditionalNamespaces = additionalNamespaces.Concat(new string[] { ContainingNamespace }).ToArray();
            Name = entityToBuildSymbol.Name;
            FullName = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
            FullNameWithConstraints = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance));
            _mockingConfiguration = mockingConfiguration;
            _fixtureConfiguration = fixtureConfiguration;
            ConstructorParameters = GetConstructorParameters(entityToBuildSymbol);
            SettableProperties = GetSetableProperties(entityToBuildSymbol);
        }

        public IEnumerable<TypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
        {
            return _uniqueTypedSymbols ??= SettableProperties
                .Where(x => !ConstructorParameters.ContainsKey(x.SymbolName))
                .Concat(ConstructorParameters.Values).ToList();
        }

        private IReadOnlyDictionary<string, TypedSymbol> GetConstructorParameters(INamedTypeSymbol entityToBuildSymbol)
        {
            return entityToBuildSymbol.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters
                .ToDictionary(x => x.PascalCaseName(), s => new TypedSymbol(s, _mockingConfiguration, _fixtureConfiguration?.Strategy));
        }

        private List<TypedSymbol> GetSetableProperties(INamedTypeSymbol entityToBuildSymbol)
        {
            var properties = entityToBuildSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(IsSetableProperty)
                .ToList();

            var propertyNames = new HashSet<string>(properties.Select(x => x.Name));

            var baseType = entityToBuildSymbol.BaseType;
            while (baseType != null)
            {
                var newProperties = baseType.GetMembers().OfType<IPropertySymbol>()
                    .Where(IsSetableProperty)
                    .Where(x => !propertyNames.Contains(x.Name)).ToList();

                properties.AddRange(newProperties);
                propertyNames.UnionWith(newProperties.Select(x => x.Name));

                baseType = baseType.BaseType;
            }

            return properties.Select(s => new TypedSymbol(s, _mockingConfiguration, _fixtureConfiguration?.Strategy)).ToList();
        }

        private static bool IsSetableProperty(IPropertySymbol x)
            => x.SetMethod is not null && x.SetMethod!.DeclaredAccessibility == Accessibility.Public && x.CanBeReferencedByName;
    }
}