using Microsoft.CodeAnalysis;

namespace Buildenator
{
    internal sealed class MakeBuilderAttributeInternal
    {
        public MakeBuilderAttributeInternal(INamedTypeSymbol typeForBuilder, string buildingMethodsPrefix)
        {
            TypeForBuilder = typeForBuilder;
            BuildingMethodsPrefix = buildingMethodsPrefix;
        }

        public INamedTypeSymbol TypeForBuilder { get; }
        public string BuildingMethodsPrefix { get; }
    }
}