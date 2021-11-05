using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;

namespace Buildenator
{
    internal sealed class MakeBuilderAttributeInternal
    {
        public MakeBuilderAttributeInternal(INamedTypeSymbol typeForBuilder, string? buildingMethodsPrefix, bool? staticCreator, NullableStrategy? nullableStrategy)
        {
            TypeForBuilder = typeForBuilder;
            BuildingMethodsPrefix = buildingMethodsPrefix;
            DefaultStaticCreator = staticCreator;
            NullableStrategy = nullableStrategy;
        }

        public INamedTypeSymbol TypeForBuilder { get; }
        public string? BuildingMethodsPrefix { get; }
        public bool? DefaultStaticCreator { get; }
        public NullableStrategy? NullableStrategy { get; }
    }
}