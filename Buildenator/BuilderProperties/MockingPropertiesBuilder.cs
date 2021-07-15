using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator
{
    internal class MockingPropertiesBuilder
    {
        private readonly ImmutableArray<TypedConstant>? _globalParameters;
        public MockingPropertiesBuilder(IAssemblySymbol context)
        {
            _globalParameters = GetMockingConfigurationOrDefault(context);
        }

        public MockingProperties? Build(ISymbol builderSymbol)
        {
            if ((GetMockingConfigurationOrDefault(builderSymbol) ?? _globalParameters) is not ImmutableArray<TypedConstant> attributeParameters)
                return null;

            var strategy = attributeParameters.GetOrThrow<MockingInterfacesStrategy>( 0, nameof(MockingProperties.Strategy));
            var typeDeclarationFormat = attributeParameters.GetOrThrow(1, nameof(MockingProperties.TypeDeclarationFormat));
            var fieldDeafultValueAssigmentFormat = attributeParameters.GetOrThrow(2, nameof(MockingProperties.FieldDeafultValueAssigmentFormat));
            var returnObjectFormat = attributeParameters.GetOrThrow(3, nameof(MockingProperties.ReturnObjectFormat));
            var additionalNamespaces = (string?)attributeParameters[4].Value;

            return new MockingProperties(
                strategy,
                typeDeclarationFormat,
                fieldDeafultValueAssigmentFormat,
                returnObjectFormat,
                additionalNamespaces?.Split(',') ?? Array.Empty<string>());
        }

        private static ImmutableArray<TypedConstant>? GetMockingConfigurationOrDefault(ISymbol context)
        {
            var attribute = context.GetAttributes().Where(x => x.AttributeClass?.BaseType?.Name == nameof(MockingConfigurationAttribute)).SingleOrDefault();
            return attribute?.ConstructorArguments;
        }
    }
}
