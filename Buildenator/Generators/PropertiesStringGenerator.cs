using System.Collections.Generic;
using System.Collections.Immutable;
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
	private readonly ImmutableDictionary<string, string>? _entityToBuilderMappings;

	public PropertiesStringGenerator(
		IBuilderProperties builder,
		IEntityToBuild entity,
		ImmutableDictionary<string, string>? entityToBuilderMappings = null)
	{
		_builder = builder;
		_entity = entity;
		_entityToBuilderMappings = entityToBuilderMappings;
	}

	public string GeneratePropertiesCode()
	{
		var properties = _entity.AllUniqueSettablePropertiesAndParameters;

		if (_builder.ShouldGenerateMethodsForUnreachableProperties || _entity.ConstructorToBuild is null)
		{
			properties = [.. properties, .. _entity.AllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch];
		}

		var output = new StringBuilder();

		GenerateFieldDeclarations(properties, output);
		GenerateWithMethods(properties, output);
		GenerateAddToMethods(properties, output);
		GenerateChildBuilderMethods(properties, output);

		return output.ToString();
	}
	
	/// <summary>
	/// Generates field declarations for all properties.
	/// </summary>
	private void GenerateFieldDeclarations(IReadOnlyList<ITypedSymbol> properties, StringBuilder output)
	{
		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredField))
		{
			output.AppendLine($@"        private {typedSymbol.GenerateLazyFieldType()} {typedSymbol.UnderScoreName}{GenerateFieldInitializer(typedSymbol)};");
		}
		
		bool IsNotYetDeclaredField(ITypedSymbol x) => !_builder.Fields.TryGetValue(x.UnderScoreName, out _);
	}
	
	/// <summary>
	/// Generates With methods for all properties.
	/// </summary>
	private void GenerateWithMethods(IReadOnlyList<ITypedSymbol> properties, StringBuilder output)
	{
		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredWithMethod))
		{
			output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}");
		}
		
		bool IsNotYetDeclaredWithMethod(ITypedSymbol x) => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x), out var methods)
		                                               || !methods.Any(method => method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.TypeName);
	}
	
	/// <summary>
	/// Generates AddTo methods for collection properties.
	/// </summary>
	private void GenerateAddToMethods(IReadOnlyList<ITypedSymbol> properties, StringBuilder output)
	{
		foreach (var typedSymbol in properties.Where(IsCollectionProperty).Where(IsNotYetDeclaredAddToMethod))
		{
			output.AppendLine($@"

        {GenerateAddToMethodDefinition(typedSymbol)}");
		}

		bool IsNotYetDeclaredAddToMethod(ITypedSymbol x)
		{
			if (!_builder.BuildingMethods.TryGetValue(CreateAddToMethodName(x), out var methods))
				return true;

			var collectionMetadata = x.GetCollectionMetadata();
			if (collectionMetadata == null)
				return true;

			// Check if any method has a matching params parameter (array or IEnumerable)
			return !methods.Any(method => 
				method.Parameters.Length == 1 && 
				method.Parameters[0].IsParams &&
				((method.Parameters[0].Type is IArrayTypeSymbol arrayType &&
				  arrayType.ElementType.Name == collectionMetadata.ElementTypeName) ||
				 (method.Parameters[0].Type is INamedTypeSymbol namedType &&
				  namedType.Name == "IEnumerable" &&
				  namedType.TypeArguments.Length == 1 &&
				  namedType.TypeArguments[0].Name == collectionMetadata.ElementTypeName)));
		}

		bool IsCollectionProperty(ITypedSymbol x) => x.GetCollectionMetadata() != null && !x.IsMockable();
	}
	
	/// <summary>
	/// Generates child builder methods for both single entities and collections.
	/// Only runs if UseChildBuilders is enabled and entity-to-builder mappings are available.
	/// </summary>
	private void GenerateChildBuilderMethods(IReadOnlyList<ITypedSymbol> properties, StringBuilder output)
	{
		if (!_builder.UseChildBuilders || _entityToBuilderMappings == null)
			return;
			
		// Generate With methods that accept Func<ChildBuilder, ChildBuilder> for single entity properties
		foreach (var typedSymbol in properties.Where(HasChildBuilder).Where(IsNotYetDeclaredChildBuilderMethod))
		{
			output.AppendLine($@"

        {GenerateChildBuilderMethodDefinition(typedSymbol)}");
		}
		
		// Generate AddTo methods that accept Func<ChildBuilder, ChildBuilder>[] for collection properties
		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredChildBuilderAddToMethod))
		{
			var collectionMetadata = typedSymbol.GetCollectionMetadata();
			if (collectionMetadata == null)
				continue;
				
			// Look up the child builder name using the element type
			if (!_entityToBuilderMappings.TryGetValue(collectionMetadata.ElementTypeDisplayName, out var childBuilderName))
				continue;
			
			output.AppendLine($@"

        {GenerateChildBuilderAddToMethodDefinition(typedSymbol, collectionMetadata, childBuilderName)}");
		}
		
		bool HasChildBuilder(ITypedSymbol x) => !x.IsMockable() && GetChildBuilderName(x) != null;
		
		bool IsNotYetDeclaredChildBuilderMethod(ITypedSymbol x)
		{
			// Check if a method with Func<ChildBuilder, ChildBuilder> signature already exists
			if (!_builder.BuildingMethods.TryGetValue(CreateMethodName(x), out var methods))
				return true;
			
			// Check if any method has a Func parameter
			return !methods.Any(method => 
				method.Parameters.Length == 1 && 
				method.Parameters[0].Type.Name.StartsWith("Func"));
		}
		
		bool IsNotYetDeclaredChildBuilderAddToMethod(ITypedSymbol x)
		{
			// Check if a method with Func<ChildBuilder, ChildBuilder> signature already exists
			if (!_builder.BuildingMethods.TryGetValue(CreateAddToMethodName(x), out var methods))
				return true;
			
			// Check if any method has a Func parameter (either params Func<>[] or params IEnumerable<Func<>>)
			return !methods.Any(method => 
				method.Parameters.Length == 1 && 
				(method.Parameters[0].Type.Name.StartsWith("Func") ||
				 (method.Parameters[0].Type is INamedTypeSymbol namedType2 &&
				  namedType2.Name == "IEnumerable" &&
				  namedType2.TypeArguments.Length == 1 &&
				  namedType2.TypeArguments[0].Name.StartsWith("Func"))));
		}
	}
	
	private string GenerateFieldInitializer(ITypedSymbol typedSymbol)
	{
		// Mockable types should not use user-defined defaults (they have their own mocking initialization)
		if (typedSymbol.IsMockable())
			return string.Empty;
		
		var defaultValueName = typedSymbol.GetDefaultValueName();
		if (defaultValueName is null)
			return string.Empty;
		
		return $" = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({defaultValueName})";
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
		
		return $"{typedSymbol.UnderScoreName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({DefaultConstants.ValueLiteral})";
	}

	private string CreateMethodName(ITypedSymbol property) => $"{_builder.BuildingMethodsPrefix}{property.SymbolPascalName}";

	private static string FieldHasValue(string fieldName)
		=> $"{fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null";

	private static string ParamsIEnumerable(string elementType, string paramName = "items")
		=> $"params System.Collections.Generic.IEnumerable<{elementType}> {paramName}";

	private string GenerateAddToMethodDefinition(ITypedSymbol typedSymbol)
	{
		var collectionMetadata = typedSymbol.GetCollectionMetadata();
		if (collectionMetadata == null)
			return string.Empty;

		var elementTypeName = collectionMetadata.ElementTypeDisplayName;
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = typedSymbol.UnderScoreName;

		if (collectionMetadata is ConcreteDictionaryMetadata concreteDictMetadata)
			return GenerateConcreteDictionaryAddTo(methodName, fieldName, typedSymbol, concreteDictMetadata);

		if (collectionMetadata is InterfaceDictionaryMetadata interfaceDictMetadata)
			return GenerateInterfaceDictionaryAddTo(methodName, fieldName, typedSymbol, interfaceDictMetadata);

		if (collectionMetadata is ArrayCollectionMetadata or ConcreteCollectionMetadata)
			return GenerateArrayOrConcreteCollectionAddTo(methodName, fieldName, typedSymbol, collectionMetadata, elementTypeName);

		// Interface collection types: use List<T> and AddRange
		return GenerateInterfaceCollectionAddTo(methodName, fieldName, typedSymbol, elementTypeName);
	}

	private string CreateAddToMethodName(ITypedSymbol property) => $"AddTo{property.SymbolPascalName}";

	private string GenerateConcreteDictionaryAddTo(string methodName, string fieldName, ITypedSymbol typedSymbol, ConcreteDictionaryMetadata meta)
	{
		var kvpType = $"System.Collections.Generic.KeyValuePair<{meta.KeyTypeDisplayName}, {meta.ValueTypeDisplayName}>";
		return $@"public {_builder.FullName} {methodName}({ParamsIEnumerable(kvpType)})
        {{
            {typedSymbol.TypeFullName} dictionary;
            if ({FieldHasValue(fieldName)})
            {{
                dictionary = {fieldName}.Value.Object;
            }}
            else
            {{
                dictionary = new {typedSymbol.NonNullableTypeFullName}();
            }}
            
            foreach (var item in items)
            {{
                dictionary[item.Key] = item.Value;
            }}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(dictionary);
            return this;
        }}";
	}

	private string GenerateInterfaceDictionaryAddTo(string methodName, string fieldName, ITypedSymbol typedSymbol, InterfaceDictionaryMetadata meta)
	{
		var kvpType = $"System.Collections.Generic.KeyValuePair<{meta.KeyTypeDisplayName}, {meta.ValueTypeDisplayName}>";
		var dictionaryType = $"System.Collections.Generic.Dictionary<{meta.KeyTypeDisplayName}, {meta.ValueTypeDisplayName}>";
		return $@"public {_builder.FullName} {methodName}({ParamsIEnumerable(kvpType)})
        {{
            var dictionary = {FieldHasValue(fieldName)}
                ? new {dictionaryType}({fieldName}.Value.Object) 
                : new {dictionaryType}();
            foreach (var item in items)
            {{
                dictionary[item.Key] = item.Value;
            }}
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(dictionary);
            return this;
        }}";
	}

	private string GenerateArrayOrConcreteCollectionAddTo(string methodName, string fieldName, ITypedSymbol typedSymbol, CollectionMetadata collectionMetadata, string elementTypeName)
	{
		var isArray = collectionMetadata is ArrayCollectionMetadata;
		var collectionVarName = isArray ? "array" : "collection";
		var collectionTypeName = isArray ? $"{elementTypeName}[]" : typedSymbol.TypeFullName;

		var addItemsCode = isArray
			? $@"var itemsArray = System.Linq.Enumerable.ToArray(items);
            if ({FieldHasValue(fieldName)})
            {{
                {collectionVarName} = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat({fieldName}.Value.Object, itemsArray));
            }}
            else
            {{
                {collectionVarName} = itemsArray;
            }}"
			: $@"if ({FieldHasValue(fieldName)})
            {{
                {collectionVarName} = {fieldName}.Value.Object;
            }}
            else
            {{
                {collectionVarName} = new {typedSymbol.NonNullableTypeFullName}();
            }}
            
            foreach (var item in items)
            {{
                {collectionVarName}.Add(item);
            }}";

		return $@"public {_builder.FullName} {methodName}({ParamsIEnumerable(elementTypeName)})
        {{
            {collectionTypeName} {collectionVarName};
            {addItemsCode}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({collectionVarName});
            return this;
        }}";
	}

	private string GenerateInterfaceCollectionAddTo(string methodName, string fieldName, ITypedSymbol typedSymbol, string elementTypeName)
	{
		return $@"public {_builder.FullName} {methodName}({ParamsIEnumerable(elementTypeName)})
        {{
            var list = {FieldHasValue(fieldName)}
                ? new System.Collections.Generic.List<{elementTypeName}>({fieldName}.Value.Object) 
                : new System.Collections.Generic.List<{elementTypeName}>();
            list.AddRange(items);
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(({typedSymbol.TypeFullName})list);
            return this;
        }}";
	}

	/// <summary>
	/// Gets the builder name for a property's type.
	/// Returns null if no child builder exists for the property's type.
	/// </summary>
	private string? GetChildBuilderName(ITypedSymbol typedSymbol)
	{
		if (_entityToBuilderMappings == null)
			return null;
		
		var typeFullName = typedSymbol.TypeFullName;
		return _entityToBuilderMappings.TryGetValue(typeFullName, out var childBuilderName) ? childBuilderName : null;
	}

	/// <summary>
	/// Generates a method that accepts Func&lt;ChildBuilder, ChildBuilder&gt; for configuring child entities.
	/// </summary>
	private string GenerateChildBuilderMethodDefinition(ITypedSymbol typedSymbol)
	{
		var childBuilderName = GetChildBuilderName(typedSymbol);
		if (childBuilderName == null)
			return string.Empty;

		var methodName = CreateMethodName(typedSymbol);
		var fieldName = typedSymbol.UnderScoreName;

		return $@"public {_builder.FullName} {methodName}(System.Func<{childBuilderName}, {childBuilderName}> configure{typedSymbol.SymbolPascalName})
        {{
            var childBuilder = new {childBuilderName}();
            childBuilder = configure{typedSymbol.SymbolPascalName}(childBuilder);
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(childBuilder.Build());
            return this;
        }}";
	}
	
	/// <summary>
	/// Generates an AddTo method that accepts Func&lt;ChildBuilder, ChildBuilder&gt; for adding child entities to a collection.
	/// </summary>
	private string GenerateChildBuilderAddToMethodDefinition(ITypedSymbol typedSymbol, CollectionMetadata collectionMetadata, string childBuilderName)
	{
		var elementTypeName = collectionMetadata.ElementTypeDisplayName;
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = typedSymbol.UnderScoreName;
		var funcType = $"System.Func<{childBuilderName}, {childBuilderName}>";
		// For array types and concrete collection types, use similar pattern
		if (collectionMetadata is ArrayCollectionMetadata or ConcreteCollectionMetadata)
		{
			var isArray = collectionMetadata is ArrayCollectionMetadata;
			var collectionVarName = isArray ? "array" : "collection";
			var collectionTypeName = isArray ? $"{elementTypeName}[]" : typedSymbol.TypeFullName;
			
			// Build child items first - for arrays we need to know the count upfront
			var buildItemsCode = isArray
				? $@"var newItemsList = new System.Collections.Generic.List<{elementTypeName}>();
            foreach (var configure in configures)
            {{
                var childBuilder = new {childBuilderName}();
                childBuilder = configure(childBuilder);
                newItemsList.Add(childBuilder.Build());
            }}
            var newItems = newItemsList.ToArray();"
				: "";
			
			var addItemsCode = isArray
				? $@"if ({FieldHasValue(fieldName)})
            {{
                {collectionVarName} = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat({fieldName}.Value.Object, newItems));
            }}
            else
            {{
                {collectionVarName} = newItems;
            }}"
				: $@"if ({FieldHasValue(fieldName)})
            {{
                {collectionVarName} = {fieldName}.Value.Object;
            }}
            else
            {{
                {collectionVarName} = new {typedSymbol.NonNullableTypeFullName}();
            }}
            
            foreach (var configure in configures)
            {{
                var childBuilder = new {childBuilderName}();
                childBuilder = configure(childBuilder);
                {collectionVarName}.Add(childBuilder.Build());
            }}";
			
			// For arrays, we need extra newlines between buildItemsCode and the variable declaration
			var methodBody = isArray
				? $@"{buildItemsCode}
            
            {collectionTypeName} {collectionVarName};
            {addItemsCode}"
				: $@"{collectionTypeName} {collectionVarName};
            {addItemsCode}";
				
			return $@"public {_builder.FullName} {methodName}({ParamsIEnumerable(funcType, "configures")})
        {{
            {methodBody}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({collectionVarName});
            return this;
        }}";
		}
		
		// For interface collection types, use List<T>
		return $@"public {_builder.FullName} {methodName}({ParamsIEnumerable(funcType, "configures")})
        {{
            var list = {FieldHasValue(fieldName)}
                ? new System.Collections.Generic.List<{elementTypeName}>({fieldName}.Value.Object) 
                : new System.Collections.Generic.List<{elementTypeName}>();
            
            foreach (var configure in configures)
            {{
                var childBuilder = new {childBuilderName}();
                childBuilder = configure(childBuilder);
                list.Add(childBuilder.Build());
            }}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(({typedSymbol.TypeFullName})list);
            return this;
        }}";
	}
}