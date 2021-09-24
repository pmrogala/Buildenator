using Microsoft.CodeAnalysis;

namespace Buildenator
{
    internal sealed class MakeBuilderAttributeInternal
    {
        public MakeBuilderAttributeInternal(INamedTypeSymbol typeForBuilder, string? buildingMethodsPrefix, bool? staticCreator)
        {
            TypeForBuilder = typeForBuilder;
            BuildingMethodsPrefix = buildingMethodsPrefix;
            DefaultStaticCreator = staticCreator;
        }

        public INamedTypeSymbol TypeForBuilder { get; }
        public string? BuildingMethodsPrefix { get; }
        public bool? DefaultStaticCreator { get; }
    }
}