using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Buildenator
{
    internal sealed class BuilderProperties
    {
        private readonly Dictionary<string, IMethodSymbol> _buildingMethods;
        private readonly Dictionary<string, IFieldSymbol> _fields;

        public BuilderProperties(INamedTypeSymbol builderSymbol, MakeBuilderAttributeInternal attributeData)
        {
            ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
            Name = builderSymbol.Name;
            FullName = builderSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
            BuildingMethodsPrefix = attributeData.BuildingMethodsPrefix ?? "With";
            NullableStrategy = attributeData.NullableStrategy ?? NullableStrategy.Default;
            StaticCreator = attributeData.DefaultStaticCreator ?? true;

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
        public string FullName { get; }
        public string BuildingMethodsPrefix { get; }
        public NullableStrategy NullableStrategy { get; }
        public bool StaticCreator { get; }

        public IReadOnlyDictionary<string, IMethodSymbol> BuildingMethods => _buildingMethods;
        public IReadOnlyDictionary<string, IFieldSymbol> Fields => _fields;
    }
}