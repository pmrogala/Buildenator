﻿using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Buildenator.Configuration
{
    internal sealed class BuilderPropertiesBuilder
    {
        private readonly string? _defaultNameWith;
        private readonly bool? _defaultStaticBuilder;
        private readonly NullableStrategy? _nullableStrategy;
        private readonly bool? _generateMethodsForUnreachableProperties;

        public BuilderPropertiesBuilder(IAssemblySymbol context)
        {
            var globalAttributes = GetConfigurationOrDefault(context);
            if (globalAttributes.HasValue)
            {
                _defaultNameWith = globalAttributes.Value.GetOrThrow<string>(0, nameof(MakeBuilderAttributeInternal.BuildingMethodsPrefix));
                _defaultStaticBuilder = globalAttributes.Value.GetOrThrow<bool>(1, nameof(MakeBuilderAttributeInternal.DefaultStaticCreator));
                _nullableStrategy = globalAttributes.Value.GetOrThrow<NullableStrategy>(2, nameof(MakeBuilderAttributeInternal.NullableStrategy));
                _generateMethodsForUnreachableProperties = globalAttributes.Value.GetOrThrow<bool>(3, nameof(MakeBuilderAttributeInternal.GenerateMethodsForUnreachableProperties));
            }
        }

        public BuilderProperties Build(INamedTypeSymbol builderSymbol, MakeBuilderAttributeInternal builderAttribute)
        {
            return new(
                builderSymbol,
                new MakeBuilderAttributeInternal(
                    builderAttribute.TypeForBuilder,
                    builderAttribute.BuildingMethodsPrefix ?? _defaultNameWith,
                    builderAttribute.DefaultStaticCreator ?? _defaultStaticBuilder,
                    builderAttribute.NullableStrategy ?? _nullableStrategy,
                    builderAttribute.GenerateMethodsForUnreachableProperties ?? _generateMethodsForUnreachableProperties));
        }

        private static ImmutableArray<TypedConstant>? GetConfigurationOrDefault(ISymbol context)
        {
            var attributeDatas = context.GetAttributes();
            var attribute = attributeDatas.SingleOrDefault(x => x.AttributeClass.HasNameOrBaseClassHas(nameof(BuildenatorConfigurationAttribute)));
            return attribute?.ConstructorArguments;
        }
    }
}