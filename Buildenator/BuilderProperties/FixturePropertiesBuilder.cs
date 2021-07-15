using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator
{
    internal class FixturePropertiesBuilder
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
            string createManyFormat = attributeParameters.GetOrThrow(i++, nameof(FixtureProperties.CreateManyFormat));
            string? constructorParameters = attributeParameters.GetOrThrow<string?>(i++, nameof(FixtureProperties.ConstructorParameters));
            string? additionalConfiguration = attributeParameters.GetOrThrow<string?>(i++, nameof(FixtureProperties.AdditionalConfiguration));
            var strategy = attributeParameters.GetOrThrow<FixtureInterfacesStrategy>(i++, nameof(FixtureProperties.Strategy));
            var additionalNamespaces = (string?)attributeParameters[i++].Value;
            return new FixtureProperties(
                name,
                createSingleFormat,
                createManyFormat,
                constructorParameters,
                additionalConfiguration,
                strategy,
                additionalNamespaces?.Split(',') ?? Array.Empty<string>());
        }

        private static ImmutableArray<TypedConstant>? GetFixtureConfigurationOrDefault(ISymbol context)
        {
            var attribute = context.GetAttributes().Where(x => x.AttributeClass?.Name == nameof(FixtureConfigurationAttribute)).SingleOrDefault();
            return attribute?.ConstructorArguments;
        }
    }
}
