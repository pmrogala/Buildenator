using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator.Configuration
{
    internal sealed class FixturePropertiesBuilder
    {
        private readonly ImmutableArray<TypedConstant>? _globalParameters;

        public FixturePropertiesBuilder(ImmutableArray<AttributeData> attributeDatas)
        {
            _globalParameters = GetFixtureConfigurationOrDefault(attributeDatas);
        }

        public FixtureProperties? Build(ISymbol builderSymbol)
        {
            if ((GetFixtureConfigurationOrDefault(builderSymbol.GetAttributes()) ?? _globalParameters) is not { } attributeParameters)
                return null;

            var i = 0;
            var name = attributeParameters.GetOrThrow(i++, nameof(FixtureProperties.Name));
            var createSingleFormat = attributeParameters.GetOrThrow(i++, nameof(FixtureProperties.CreateSingleFormat));
            var constructorParameters = (string?)attributeParameters[i++].Value;
            var additionalConfiguration = (string?)attributeParameters[i++].Value;
            var strategy = attributeParameters.GetOrThrow<FixtureInterfacesStrategy>(i++, nameof(FixtureProperties.Strategy));
            var additionalNamespaces = (string?)attributeParameters[i].Value;
            return new FixtureProperties(
                name,
                createSingleFormat,
                constructorParameters,
                additionalConfiguration,
                strategy,
                additionalNamespaces?.Split(',') ?? Array.Empty<string>());
        }

        private static ImmutableArray<TypedConstant>? GetFixtureConfigurationOrDefault(ImmutableArray<AttributeData> attributeData)
        {
            var attribute = attributeData.SingleOrDefault(x => x.AttributeClass.HasNameOrBaseClassHas(nameof(FixtureConfigurationAttribute)));
            return attribute?.ConstructorArguments;
        }
    }
}
