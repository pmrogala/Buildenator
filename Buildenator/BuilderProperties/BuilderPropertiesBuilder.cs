using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator
{
    internal sealed class BuilderPropertiesBuilder
    {
        private readonly string? _defaultNameWith;
        private readonly bool? _defaultStaticBuilder;

        public BuilderPropertiesBuilder(IAssemblySymbol context)
        {
            var globalAttributes = GetConfigurationOrDefault(context);
            _defaultNameWith = (string?)globalAttributes?[0].Value;
            _defaultStaticBuilder = (bool?)globalAttributes?[1].Value;
        }

        public BuilderProperties Build(INamedTypeSymbol builderSymbol, MakeBuilderAttributeInternal builderAttribute)
        {
            return new(
                builderSymbol,
                new MakeBuilderAttributeInternal(
                    builderAttribute.TypeForBuilder,
                    builderAttribute.BuildingMethodsPrefix ?? _defaultNameWith,
                    builderAttribute.DefaultStaticCreator ?? _defaultStaticBuilder));
        }

        private static ImmutableArray<TypedConstant>? GetConfigurationOrDefault(ISymbol context)
        {
            var attributeDatas = context.GetAttributes();
            var attribute = attributeDatas.Where(x => x.AttributeClass?.BaseType?.Name == nameof(BuildenatorConfigurationAttribute)).SingleOrDefault();
            return attribute?.ConstructorArguments;
        }
    }
}