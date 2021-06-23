using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator
{
    internal class FixtureConfigurationBuilder
    {
        private ImmutableArray<TypedConstant>? _attributeParameters;
        public FixtureConfigurationBuilder(IAssemblySymbol context)
        {
            _attributeParameters = GetFixtureConfigurationOrDefault(context);
        }

        public FixtureConfiguration Build(ISymbol builderSymbol)
        {
            _attributeParameters = GetFixtureConfigurationOrDefault(builderSymbol) ?? _attributeParameters;
            var fixture = (ITypeSymbol?)_attributeParameters?[0].Value;
            var additionalNamespaces = (string?)_attributeParameters?[1].Value;
            return new FixtureConfiguration(
                fixture?.Name ?? "Fixture",
                fixture?.ContainingNamespace.ToDisplayString() ?? "AutoFixture",
                additionalNamespaces?.Split(',') ?? Array.Empty<string>());
        }

        private static ImmutableArray<TypedConstant>? GetFixtureConfigurationOrDefault(ISymbol context)
        {
            var attribute = context.GetAttributes().Where(x => x.AttributeClass?.Name == nameof(FixtureConfigurationAttribute)).SingleOrDefault();
            return attribute?.ConstructorArguments;
        }
    }

    internal class FixtureConfiguration
    {
        public FixtureConfiguration(string name, string @namespace, string[] additionalNamespaces)
        {
            Name = name;
            Namespace = @namespace;
            AdditionalNamespaces = additionalNamespaces;
        }

        public string Name { get; }
        public string Namespace { get; }
        public string[] AdditionalNamespaces { get; }

    }
}
