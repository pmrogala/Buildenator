using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator
{
    internal class EntityToBuildProperties
    {
        public string ContainingNamespace { get; }
        public string Name { get; }
        public IReadOnlyDictionary<string, IParameterSymbol> ConstructorParameters { get; }
        public IEnumerable<IPropertySymbol> SettableProperties { get; }

        public EntityToBuildProperties(INamedTypeSymbol entityToBuildSymbol)
        {
            ContainingNamespace = entityToBuildSymbol.ContainingNamespace.ToDisplayString();
            Name = entityToBuildSymbol.Name;
            ConstructorParameters = GetConstructorParameters(entityToBuildSymbol);
            SettableProperties = GetSetableProperties(entityToBuildSymbol);
        }

        public IEnumerable<(ISymbol Property, ITypeSymbol Type)> GetAllUniqueSettablePropertiesAndParameters()
        {
            var parameters = ConstructorParameters;
            return SettableProperties
                .Where(x => !parameters.ContainsKey(x.Name))
                .Select(x => ((ISymbol)x, x.Type))
                .Concat(parameters.Values.Select(x => ((ISymbol)x, x.Type)));
        }

        private IReadOnlyDictionary<string, IParameterSymbol> GetConstructorParameters(INamedTypeSymbol entityToBuildSymbol)
        {
            var properties = entityToBuildSymbol.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters;

            return properties.ToDictionary(x => x.PascalCaseName());
        }


        private IEnumerable<IPropertySymbol> GetSetableProperties(INamedTypeSymbol entityToBuildSymbol)
        {
            var properties = entityToBuildSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.SetMethod is not null)
                .Where(x => x.SetMethod!.DeclaredAccessibility == Accessibility.Public)
                .Where(x => x.CanBeReferencedByName).ToList();

            var propertyNames = new HashSet<string>(properties.Select(x => x.Name));

            var baseType = entityToBuildSymbol.BaseType;

            while (baseType != null)
            {
                var newProperties = baseType.GetMembers().OfType<IPropertySymbol>()
                                            .Where(x => x.CanBeReferencedByName)
                                            .Where(x => x.SetMethod is not null)
                                            .Where(x => x.SetMethod!.DeclaredAccessibility == Accessibility.Public)
                                            .Where(x => !propertyNames.Contains(x.Name)).ToList();
                properties.AddRange(newProperties);
                propertyNames.UnionWith(newProperties.Select(x => x.Name));

                baseType = baseType.BaseType;
            }

            return properties;
        }
    }
}