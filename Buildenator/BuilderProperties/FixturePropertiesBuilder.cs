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

            var fixture = attributeParameters.GetOrThrow<ITypeSymbol>(0, "Fixture");
            var strategy = attributeParameters.GetOrThrow<FixtureInterfacesStrategy>(1, nameof(FixtureProperties.Strategy));
            var additionalNamespaces = (string?)attributeParameters[2].Value;
            return new FixtureProperties(
                fixture.Name,
                fixture.ContainingNamespace.ToDisplayString(),
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
