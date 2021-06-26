using Microsoft.CodeAnalysis;

namespace Buildenator
{
    internal class BuilderProperties
    {
        public BuilderProperties(INamedTypeSymbol builderSymbol)
        {
            ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
            Name = builderSymbol.Name;
        }

        public string ContainingNamespace { get; }
        public string Name { get; }
    }
}