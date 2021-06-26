using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator
{
    internal class BuilderProperties
    {
        public BuilderProperties(INamedTypeSymbol builderSymbol, AttributeData attributeData)
        {
            ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
            Name = builderSymbol.Name;
            BuildingMethodsPrefix = (string)attributeData.ConstructorArguments[1].Value!;

            if (string.IsNullOrWhiteSpace(BuildingMethodsPrefix))
                throw new ArgumentNullException(nameof(attributeData), "Prefix name shouldn't be empty!");

            var memebers = builderSymbol.GetMembers();
            BuildingMethods = memebers.OfType<IMethodSymbol>()
                .Where(x => x.Name.StartsWith(BuildingMethodsPrefix))
                .ToDictionary(x => x.Name);
            Fields = memebers.OfType<IFieldSymbol>().ToDictionary(x => x.Name);
        }

        public string ContainingNamespace { get; }
        public string Name { get; }
        public string BuildingMethodsPrefix { get; }
        public IReadOnlyDictionary<string, IMethodSymbol> BuildingMethods { get; }
        public IReadOnlyDictionary<string, IFieldSymbol> Fields { get; }
    }
}