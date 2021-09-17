﻿using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator
{
    internal class EntityToBuildProperties
    {
        private IEnumerable<TypedSymbol>? _uniqueTypedSymbols;
        private readonly MockingProperties? _mockingConfiguration;
        private readonly FixtureProperties? _fixtureConfiguration;

        public string ContainingNamespace { get; }
        public string Name { get; }
        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }
        public IEnumerable<TypedSymbol> SettableProperties { get; }

        public EntityToBuildProperties(MakeBuilderAttributeInternal attribute, MockingProperties? mockingConfiguration, FixtureProperties? fixtureConfiguration)
        {
            var entityToBuildSymbol = attribute.TypeForBuilder;
            ContainingNamespace = entityToBuildSymbol.ContainingNamespace.ToDisplayString();
            Name = entityToBuildSymbol.Name;
            _mockingConfiguration = mockingConfiguration;
            _fixtureConfiguration = fixtureConfiguration;
            ConstructorParameters = GetConstructorParameters(entityToBuildSymbol);
            SettableProperties = GetSetableProperties(entityToBuildSymbol);
        }

        public IEnumerable<TypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
        {
            return _uniqueTypedSymbols ??= SettableProperties
                .Where(x => !ConstructorParameters.ContainsKey(x.Symbol.Name))
                .Concat(ConstructorParameters.Values).ToList();
        }

        private IReadOnlyDictionary<string, TypedSymbol> GetConstructorParameters(INamedTypeSymbol entityToBuildSymbol)
        {
            return entityToBuildSymbol.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters
                .ToDictionary(x => x.PascalCaseName(), s => new TypedSymbol(s, _mockingConfiguration, _fixtureConfiguration));
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

            return properties.Select(s => new TypedSymbol(s, _mockingConfiguration, _fixtureConfiguration)).ToList();
        }

        private static bool IsSetableProperty(IPropertySymbol x)
            => x.SetMethod is not null && x.SetMethod!.DeclaredAccessibility == Accessibility.Public && x.CanBeReferencedByName;
    }
}