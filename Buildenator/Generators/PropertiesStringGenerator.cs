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
            output = output.AppendLine($@"        private {typedSymbol.GenerateLazyFieldType()} {typedSymbol.UnderScoreName};");
		}

		// Generate fields to track items to add for collection properties
		foreach (var typedSymbol in allPropertiesForAddMethods.Where(IsCollectionProperty))
		{
			var elementType = CollectionMethodDetector.GetCollectionElementType(typedSymbol.TypeSymbol);
			if (elementType != null)
			{
				var elementTypeName = elementType.ToDisplayString();
				var fieldName = GetAddItemsFieldName(typedSymbol);
				output = output.AppendLine($@"        private System.Collections.Generic.List<{elementTypeName}> {fieldName} = new System.Collections.Generic.List<{elementTypeName}>();");
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

		bool IsCollectionProperty(ITypedSymbol x) => CollectionMethodDetector.IsCollectionProperty(x, x.TypeSymbol);
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
		=> typedSymbol.IsMockable()
			? $"{DefaultConstants.SetupActionLiteral}({typedSymbol.UnderScoreName})"
			: $"{typedSymbol.UnderScoreName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({DefaultConstants.ValueLiteral})";

	private string CreateMethodName(ITypedSymbol property) => $"{_builder.BuildingMethodsPrefix}{property.SymbolPascalName}";

	private string GenerateAddToMethodDefinition(ITypedSymbol typedSymbol)
	{
		var elementType = CollectionMethodDetector.GetCollectionElementType(typedSymbol.TypeSymbol);
		if (elementType == null)
			return string.Empty;

		var elementTypeName = elementType.ToDisplayString();
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = GetAddItemsFieldName(typedSymbol);
		
		return $@"public {_builder.FullName} {methodName}(params {elementTypeName}[] items)
        {{
            if ({fieldName} == null)
            {{
                {fieldName} = new System.Collections.Generic.List<{elementTypeName}>();
            }}
            {fieldName}.AddRange(items);
            return this;
        }}";
	}

	private string CreateAddToMethodName(ITypedSymbol property) => $"AddTo{property.SymbolPascalName}";

	private string GetAddItemsFieldName(ITypedSymbol property) => $"_{property.SymbolName}ToAdd";
}