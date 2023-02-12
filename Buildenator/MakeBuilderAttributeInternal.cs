using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;

namespace Buildenator
{
	internal sealed class MakeBuilderAttributeInternal
	{
		public MakeBuilderAttributeInternal(AttributeData attribute)
			: this(
				(INamedTypeSymbol)attribute.ConstructorArguments[0].Value!,
				(string?)attribute.ConstructorArguments[1].Value,
				(bool?)attribute.ConstructorArguments[2].Value,
				attribute.ConstructorArguments[3].Value is null
					? null
					: (NullableStrategy)attribute.ConstructorArguments[3].Value!,
				(bool?)attribute.ConstructorArguments[4].Value,
				(bool?)attribute.ConstructorArguments[5].Value)
		{

		}

		public MakeBuilderAttributeInternal(
			INamedTypeSymbol typeForBuilder, string? buildingMethodsPrefix, bool? staticCreator, NullableStrategy? nullableStrategy, bool? generateMethodsForUnreachableProperties, bool? implicitCast)
		{
			TypeForBuilder = typeForBuilder;
			BuildingMethodsPrefix = buildingMethodsPrefix;
			DefaultStaticCreator = staticCreator;
			NullableStrategy = nullableStrategy;
			GenerateMethodsForUnreachableProperties = generateMethodsForUnreachableProperties;
			ImplicitCast = implicitCast;
		}

		public INamedTypeSymbol TypeForBuilder { get; }
		public string? BuildingMethodsPrefix { get; }
		public bool? DefaultStaticCreator { get; }
		public bool? ImplicitCast { get; }
		public NullableStrategy? NullableStrategy { get; }
		public bool? GenerateMethodsForUnreachableProperties { get; }
	}
}