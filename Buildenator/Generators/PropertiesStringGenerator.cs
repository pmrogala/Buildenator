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

		// Generate fields to track items to add for properties with Add methods
		foreach (var typedSymbol in allPropertiesForAddMethods.Where(HasAddMethod))
		{
			var parameterType = CollectionMethodDetector.GetAddMethodParameterType(_entity.EntitySymbol, typedSymbol);
			if (parameterType != null)
			{
				var parameterTypeName = parameterType.ToDisplayString();
				var fieldName = GetAddItemsFieldName(typedSymbol);
				output = output.AppendLine($@"        private System.Collections.Generic.List<{parameterTypeName}> {fieldName} = new System.Collections.Generic.List<{parameterTypeName}>();");
			}
		}

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredMethod))
		{
            output = output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}");

		}

		// Generate AddTo methods for properties with Add methods
		foreach (var typedSymbol in allPropertiesForAddMethods.Where(HasAddMethod))
		{
			output = output.AppendLine($@"

        {GenerateAddToMethodDefinition(typedSymbol)}");
		}

		return output.ToString();

		bool IsNotYetDeclaredField(ITypedSymbol x) => !_builder.Fields.TryGetValue(x.UnderScoreName, out _);

		bool IsNotYetDeclaredMethod(ITypedSymbol x) => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x), out var method)
		                                               || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.TypeName);

		bool HasAddMethod(ITypedSymbol x) => CollectionMethodDetector.HasAddMethodForProperty(_entity.EntitySymbol, x);
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
		var parameterType = CollectionMethodDetector.GetAddMethodParameterType(_entity.EntitySymbol, typedSymbol);
		if (parameterType == null)
			return string.Empty;

		var parameterTypeName = parameterType.ToDisplayString();
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = GetAddItemsFieldName(typedSymbol);
		
		return $@"public {_builder.FullName} {methodName}({parameterTypeName} item)
        {{
            {fieldName}.Add(item);
            return this;
        }}";
	}

	private string CreateAddToMethodName(ITypedSymbol property) => $"AddTo{property.SymbolPascalName}";

	private string GetAddItemsFieldName(ITypedSymbol property) => $"_{property.SymbolName}ToAdd";

	private static string GetSingularPropertyName(string propertyName)
	{
		// Simple heuristic: remove trailing 's' if present
		if (propertyName.Length > 1 && propertyName.EndsWith("s") && !propertyName.EndsWith("ss"))
		{
			return propertyName.Substring(0, propertyName.Length - 1);
		}
		
		return propertyName;
	}
}