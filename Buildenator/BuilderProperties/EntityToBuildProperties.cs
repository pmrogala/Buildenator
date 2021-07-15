using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator
{
    internal class EntityToBuildProperties
    {
        private IEnumerable<TypedSymbol>? _uniqueTypedSymbols;
        public string ContainingNamespace { get; }
        public string Name { get; }
        public IReadOnlyDictionary<string, IParameterSymbol> ConstructorParameters { get; }
        public IEnumerable<IPropertySymbol> SettableProperties { get; }

        public EntityToBuildProperties(MakeBuilderAttributeInternal attribute)
        {
            var entityToBuildSymbol = attribute.TypeForBuilder;
            ContainingNamespace = entityToBuildSymbol.ContainingNamespace.ToDisplayString();
            Name = entityToBuildSymbol.Name;
            ConstructorParameters = GetConstructorParameters(entityToBuildSymbol);
            SettableProperties = GetSetableProperties(entityToBuildSymbol);
        }

        public IEnumerable<TypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
        {
            return _uniqueTypedSymbols ??= SettableProperties
                .Where(x => !ConstructorParameters.ContainsKey(x.Name))
                .Select(x => new TypedSymbol(x))
                .Concat(ConstructorParameters.Values.Select(x => new TypedSymbol(x))).ToList();
        }

        private IReadOnlyDictionary<string, IParameterSymbol> GetConstructorParameters(INamedTypeSymbol entityToBuildSymbol)
        {
            return entityToBuildSymbol.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters
                .ToDictionary(x => x.PascalCaseName());
        }

        private IEnumerable<IPropertySymbol> GetSetableProperties(INamedTypeSymbol entityToBuildSymbol)
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

            return properties;
        }

        private static bool IsSetableProperty(IPropertySymbol x)
            => x.SetMethod is not null && x.SetMethod!.DeclaredAccessibility == Accessibility.Public && x.CanBeReferencedByName;
    }
}