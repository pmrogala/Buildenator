using System.Linq;
using System.Text;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration;
using Buildenator.Configuration.Contract;
using Microsoft.CodeAnalysis;

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

		var output = new StringBuilder();

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredField))
		{
            output = output.AppendLine($@"        private {typedSymbol.GenerateLazyFieldType()} {typedSymbol.UnderScoreName};");
		}

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredWithMethod))
		{
            output = output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}");

		}

		// Generate AddTo methods for collection properties
		foreach (var typedSymbol in properties.Where(IsCollectionProperty).Where(IsNotYetDeclaredAddToMethod))
		{
			output = output.AppendLine($@"

        {GenerateAddToMethodDefinition(typedSymbol)}");
		}

		return output.ToString();

		bool IsNotYetDeclaredField(ITypedSymbol x) => !_builder.Fields.TryGetValue(x.UnderScoreName, out _);

		bool IsNotYetDeclaredWithMethod(ITypedSymbol x) => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x), out var methods)
		                                               || !methods.Any(method => method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.TypeName);

		bool IsNotYetDeclaredAddToMethod(ITypedSymbol x)
		{
			if (!_builder.BuildingMethods.TryGetValue(CreateAddToMethodName(x), out var methods))
				return true;

			var collectionMetadata = x.GetCollectionMetadata();
			if (collectionMetadata == null)
				return true;

			var elementTypeName = collectionMetadata.ElementType.Name;
			// Check if any method has a matching params array parameter
			return !methods.Any(method => 
				method.Parameters.Length == 1 && 
				method.Parameters[0].IsParams &&
				method.Parameters[0].Type is IArrayTypeSymbol arrayType &&
				arrayType.ElementType.Name == elementTypeName);
		}

		bool IsCollectionProperty(ITypedSymbol x) => x.GetCollectionMetadata() != null && !x.IsMockable();
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
		var collectionMetadata = typedSymbol.GetCollectionMetadata();
		if (collectionMetadata == null)
			return string.Empty;

		var elementTypeName = collectionMetadata.ElementType.ToDisplayString();
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = typedSymbol.UnderScoreName;
		
		// For concrete types, use new() and .Add() method from ICollection<T>
		if (collectionMetadata is ConcreteCollectionMetadata)
		{
			return $@"public {_builder.FullName} {methodName}(params {elementTypeName}[] items)
        {{
            {typedSymbol.TypeFullName} collection;
            if ({fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null)
            {{
                collection = {fieldName}.Value.Object;
            }}
            else
            {{
                collection = new {typedSymbol.TypeFullName}();
            }}
            
            foreach (var item in items)
            {{
                collection.Add(item);
            }}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(collection);
            return this;
        }}";
		}
		
		// For interface types, use List<T> and AddRange
		return $@"public {_builder.FullName} {methodName}(params {elementTypeName}[] items)
        {{
            var list = {fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null
                ? new System.Collections.Generic.List<{elementTypeName}>({fieldName}.Value.Object) 
                : new System.Collections.Generic.List<{elementTypeName}>();
            list.AddRange(items);
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>((({typedSymbol.TypeFullName})list));
            return this;
        }}";
	}

	private string CreateAddToMethodName(ITypedSymbol property) => $"AddTo{property.SymbolPascalName}";
}