using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator.Configuration
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
            if ((GetMockingConfigurationOrDefault(builderSymbol) ?? _globalParameters) is not { } attributeParameters)
                return null;

            var strategy = attributeParameters.GetOrThrow<MockingInterfacesStrategy>(0, nameof(MockingProperties.Strategy));
            var typeDeclarationFormat = attributeParameters.GetOrThrow(1, nameof(MockingProperties.TypeDeclarationFormat));
            var defaultValueAssignmentFormat = attributeParameters.GetOrThrow(2, nameof(MockingProperties.FieldDefaultValueAssignmentFormat));
            var returnObjectFormat = attributeParameters.GetOrThrow(3, nameof(MockingProperties.ReturnObjectFormat));
            var additionalNamespaces = (string?)attributeParameters[4].Value;

            return new MockingProperties(
                strategy,
                typeDeclarationFormat,
                defaultValueAssignmentFormat,
                returnObjectFormat,
                additionalNamespaces?.Split(',') ?? Array.Empty<string>());
        }

        private static ImmutableArray<TypedConstant>? GetMockingConfigurationOrDefault(ISymbol context)
        {
            var attributeData = context.GetAttributes();
            var attribute = attributeData.SingleOrDefault(x => x.AttributeClass.HasNameOrBaseClassHas(nameof(MockingConfigurationAttribute)));
            return attribute?.ConstructorArguments;
        }
    }
}
