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
            if ((GetMockingConfigurationOrDefault(builderSymbol) ?? _globalParameters) is not ImmutableArray<TypedConstant> attributeParameters)
                return null;

            var strategy = attributeParameters.GetOrThrow<MockingInterfacesStrategy>(0, nameof(MockingProperties.Strategy));
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
            var attributeDatas = context.GetAttributes();
            var attribute = attributeDatas.Where(x => x.AttributeClass.HasNameOrBaseClassHas(nameof(MockingConfigurationAttribute))).SingleOrDefault();
            return attribute?.ConstructorArguments;
        }
    }
}
