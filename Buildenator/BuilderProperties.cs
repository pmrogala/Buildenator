using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator
{
    internal class BuilderProperties
    {
        private readonly Dictionary<string, IMethodSymbol> _buildingMethods;
        private readonly Dictionary<string, IFieldSymbol> _fields;

        public BuilderProperties(INamedTypeSymbol builderSymbol, MakeBuilderAttributeInternal attributeData)
        {
            ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
            Name = builderSymbol.Name;
            BuildingMethodsPrefix = attributeData.BuildingMethodsPrefix;

            if (string.IsNullOrWhiteSpace(BuildingMethodsPrefix))
                throw new ArgumentNullException(nameof(attributeData), "Prefix name shouldn't be empty!");

            _buildingMethods = new Dictionary<string, IMethodSymbol>();
            _fields = new Dictionary<string, IFieldSymbol>();
            foreach (var member in builderSymbol.GetMembers())
            {
                if (member is IMethodSymbol method && method.Name.StartsWith(BuildingMethodsPrefix))
                    _buildingMethods.Add(method.Name, method);
                else if (member is IFieldSymbol field)
                    _fields.Add(field.Name, field);
            }
        }

        public string ContainingNamespace { get; }
        public string Name { get; }
        public string BuildingMethodsPrefix { get; }
        public IReadOnlyDictionary<string, IMethodSymbol> BuildingMethods => _buildingMethods;
        public IReadOnlyDictionary<string, IFieldSymbol> Fields => _fields;
    }
}