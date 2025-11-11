using System.Linq;
using System.Text;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration;
using Buildenator.Configuration.Contract;

namespace Buildenator.Generators;

internal sealed class PropertiesStringGenerator
{
	private readonly IBuilderProperties _builder;
	private readonly IEntityToBuild _entity;

	public PropertiesStringGenerator(IBuilderProperties builder, IEntityToBuild entity)
	{
		_builder = builder;
		_entity = entity;
	}

	public string GeneratePropertiesCode()
	{
		var properties = _entity.AllUniqueSettablePropertiesAndParameters;

		if (_builder.ShouldGenerateMethodsForUnreachableProperties || _entity.ConstructorToBuild is null)
		{
			properties = [.. properties, .. _entity.AllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch];
		}

		// For AddTo methods, we need to check ALL properties (including read-only ones)
		var allPropertiesForAddMethods = _entity.AllUniqueSettablePropertiesAndParameters
			.Concat(_entity.AllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch)
			.ToList();

		var output = new StringBuilder();

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredField))
		{
			if (typedSymbol.IsCollection && !typedSymbol.IsMockable() && typedSymbol.CollectionElementType != null)
			{
				var elementTypeName = typedSymbol.CollectionElementType.ToDisplayString();
				output = output.AppendLine($@"        private {typedSymbol.GenerateLazyFieldType()} {typedSymbol.UnderScoreName} = new System.Collections.Generic.List<{elementTypeName}>();");
			}
			else
			{
				output = output.AppendLine($@"        private {typedSymbol.GenerateLazyFieldType()} {typedSymbol.UnderScoreName};");
			}
		}

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredMethod))
		{
            output = output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}");

		}

		// Generate AddTo methods for collection properties
		foreach (var typedSymbol in allPropertiesForAddMethods.Where(IsCollectionProperty))
		{
			output = output.AppendLine($@"

        {GenerateAddToMethodDefinition(typedSymbol)}");
		}

		return output.ToString();

		bool IsNotYetDeclaredField(ITypedSymbol x) => !_builder.Fields.TryGetValue(x.UnderScoreName, out _);

		bool IsNotYetDeclaredMethod(ITypedSymbol x) => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x), out var method)
		                                               || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.TypeName);

		bool IsCollectionProperty(ITypedSymbol x) => x.IsCollection && !x.IsMockable();
	}

	private string GenerateMethodDefinition(ITypedSymbol typedSymbol)
		=> $@"{GenerateMethodDefinitionHeader(typedSymbol)}
        {{
            {GenerateValueAssignment(typedSymbol)};
            return this;
        }}";

	private string GenerateMethodDefinitionHeader(ITypedSymbol typedSymbol)
		=> $"public {_builder.FullName} {CreateMethodName(typedSymbol)}({typedSymbol.GenerateMethodParameterDefinition()})";

	private static string GenerateValueAssignment(ITypedSymbol typedSymbol)
	{
		if (typedSymbol.IsMockable())
			return $"{DefaultConstants.SetupActionLiteral}({typedSymbol.UnderScoreName})";
		
		if (typedSymbol.IsCollection && typedSymbol.CollectionElementType != null && !typedSymbol.IsMockable())
		{
			var elementTypeName = typedSymbol.CollectionElementType.ToDisplayString();
			// For collections, clear and replace with new items from the value parameter
			return $"{typedSymbol.UnderScoreName}.Clear(); if ({DefaultConstants.ValueLiteral} != null) {{ {typedSymbol.UnderScoreName}.AddRange({DefaultConstants.ValueLiteral}); }}";
		}
		
		return $"{typedSymbol.UnderScoreName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({DefaultConstants.ValueLiteral})";
	}

	private string CreateMethodName(ITypedSymbol property) => $"{_builder.BuildingMethodsPrefix}{property.SymbolPascalName}";

	private string GenerateAddToMethodDefinition(ITypedSymbol typedSymbol)
	{
		var elementType = typedSymbol.CollectionElementType;
		if (elementType == null)
			return string.Empty;

		var elementTypeName = elementType.ToDisplayString();
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = typedSymbol.UnderScoreName;
		
		return $@"public {_builder.FullName} {methodName}(params {elementTypeName}[] items)
        {{
            {fieldName}.AddRange(items);
            return this;
        }}";
	}

	private string CreateAddToMethodName(ITypedSymbol property) => $"AddTo{property.SymbolPascalName}";
}