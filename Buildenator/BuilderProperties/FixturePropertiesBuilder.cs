using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator
{
    internal sealed class FixturePropertiesBuilder
    {
        private readonly ImmutableArray<TypedConstant>? _globalParameters;
        public FixturePropertiesBuilder(IAssemblySymbol context)
        {
            _globalParameters = GetFixtureConfigurationOrDefault(context);
        }

        public FixtureProperties? Build(ISymbol builderSymbol)
        {
            if ((GetFixtureConfigurationOrDefault(builderSymbol) ?? _globalParameters) is not ImmutableArray<TypedConstant> attributeParameters)
                return null;

            var i = 0;
            var name = attributeParameters.GetOrThrow(i++, nameof(FixtureProperties.Name));
            string createSingleFormat = attributeParameters.GetOrThrow(i++, nameof(FixtureProperties.CreateSingleFormat));
            string? constructorParameters = (string?)attributeParameters[i++].Value;
            string? additionalConfiguration = (string?)attributeParameters[i++].Value;
            var strategy = attributeParameters.GetOrThrow<FixtureInterfacesStrategy>(i++, nameof(FixtureProperties.Strategy));
            var additionalNamespaces = (string?)attributeParameters[i++].Value;
            return new FixtureProperties(
                name,
                createSingleFormat,
                constructorParameters,
                additionalConfiguration,
                strategy,
                additionalNamespaces?.Split(',') ?? Array.Empty<string>());
        }

        private static ImmutableArray<TypedConstant>? GetFixtureConfigurationOrDefault(ISymbol context)
        {
            var attributeDatas = context.GetAttributes();
            var attribute = attributeDatas.Where(x => x.AttributeClass?.BaseType?.Name == nameof(FixtureConfigurationAttribute)).SingleOrDefault();
            return attribute?.ConstructorArguments;
        }
    }
}
