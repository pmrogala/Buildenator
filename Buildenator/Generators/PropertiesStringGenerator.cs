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

			// Check if any method has a matching params array parameter
			return !methods.Any(method => 
				method.Parameters.Length == 1 && 
				method.Parameters[0].IsParams &&
				method.Parameters[0].Type is IArrayTypeSymbol arrayType &&
				arrayType.ElementType.Name == collectionMetadata.ElementTypeName);
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
			
			// Check if any method has a Func parameter
			return !methods.Any(method => 
				method.Parameters.Length == 1 && 
				method.Parameters[0].Type.Name.StartsWith("Func"));
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
		=> typedSymbol.IsMockable()
			? $"{DefaultConstants.SetupActionLiteral}({typedSymbol.UnderScoreName})"
			: $"{typedSymbol.UnderScoreName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({DefaultConstants.ValueLiteral})";

	private string CreateMethodName(ITypedSymbol property) => $"{_builder.BuildingMethodsPrefix}{property.SymbolPascalName}";

	private string GenerateAddToMethodDefinition(ITypedSymbol typedSymbol)
	{
		var collectionMetadata = typedSymbol.GetCollectionMetadata();
		if (collectionMetadata == null)
			return string.Empty;

		var elementTypeName = collectionMetadata.ElementTypeDisplayName;
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = typedSymbol.UnderScoreName;
		
		// For concrete dictionary types, use Dictionary's indexer for adding
		if (collectionMetadata is ConcreteDictionaryMetadata concreteDictMetadata)
		{
			var keyTypeName = concreteDictMetadata.KeyTypeDisplayName;
			var valueTypeName = concreteDictMetadata.ValueTypeDisplayName;
			return $@"public {_builder.FullName} {methodName}(params System.Collections.Generic.KeyValuePair<{keyTypeName}, {valueTypeName}>[] items)
        {{
            {typedSymbol.TypeFullName} dictionary;
            if ({fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null)
            {{
                dictionary = {fieldName}.Value.Object;
            }}
            else
            {{
                dictionary = new {typedSymbol.TypeFullName}();
            }}
            
            foreach (var item in items)
            {{
                dictionary[item.Key] = item.Value;
            }}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(dictionary);
            return this;
        }}";
		}
		
		// For interface dictionary types, create a new Dictionary and use indexer
		if (collectionMetadata is InterfaceDictionaryMetadata interfaceDictMetadata)
		{
			var keyTypeName = interfaceDictMetadata.KeyTypeDisplayName;
			var valueTypeName = interfaceDictMetadata.ValueTypeDisplayName;
			var dictionaryType = $"System.Collections.Generic.Dictionary<{keyTypeName}, {valueTypeName}>";
			return $@"public {_builder.FullName} {methodName}(params System.Collections.Generic.KeyValuePair<{keyTypeName}, {valueTypeName}>[] items)
        {{
            var dictionary = {fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null
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
		
		// For array types and concrete collection types, use similar pattern
		if (collectionMetadata is ArrayCollectionMetadata or ConcreteCollectionMetadata)
		{
			var isArray = collectionMetadata is ArrayCollectionMetadata;
			var collectionVarName = isArray ? "array" : "collection";
			var collectionTypeName = isArray ? $"{elementTypeName}[]" : typedSymbol.TypeFullName;
			
			var addItemsCode = isArray
				? $@"if ({fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null)
            {{
                var existingArray = {fieldName}.Value.Object;
                {collectionVarName} = new {elementTypeName}[existingArray.Length + items.Length];
                System.Array.Copy(existingArray, 0, {collectionVarName}, 0, existingArray.Length);
                System.Array.Copy(items, 0, {collectionVarName}, existingArray.Length, items.Length);
            }}
            else
            {{
                {collectionVarName} = items;
            }}"
				: $@"if ({fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null)
            {{
                {collectionVarName} = {fieldName}.Value.Object;
            }}
            else
            {{
                {collectionVarName} = new {typedSymbol.TypeFullName}();
            }}
            
            foreach (var item in items)
            {{
                {collectionVarName}.Add(item);
            }}";
			
			return $@"public {_builder.FullName} {methodName}(params {elementTypeName}[] items)
        {{
            {collectionTypeName} {collectionVarName};
            {addItemsCode}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({collectionVarName});
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
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(({typedSymbol.TypeFullName})list);
            return this;
        }}";
	}

	private string CreateAddToMethodName(ITypedSymbol property) => $"AddTo{property.SymbolPascalName}";

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
		var entityTypeName = typedSymbol.TypeFullName;

		return $@"public {_builder.FullName} {methodName}(System.Func<{childBuilderName}, {childBuilderName}> configure{typedSymbol.SymbolPascalName})
        {{
            var childBuilder = new {childBuilderName}();
            childBuilder = configure{typedSymbol.SymbolPascalName}(childBuilder);
            {fieldName} = new {DefaultConstants.NullBox}<{entityTypeName}>(childBuilder.Build());
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
		
		// For array types and concrete collection types, use similar pattern
		if (collectionMetadata is ArrayCollectionMetadata or ConcreteCollectionMetadata)
		{
			var isArray = collectionMetadata is ArrayCollectionMetadata;
			var collectionVarName = isArray ? "array" : "collection";
			var collectionTypeName = isArray ? $"{elementTypeName}[]" : typedSymbol.TypeFullName;
			
			// Build child items first - for arrays we need to know the count upfront
			var buildItemsCode = isArray
				? $@"var newItems = new {elementTypeName}[configures.Length];
            for (int i = 0; i < configures.Length; i++)
            {{
                var childBuilder = new {childBuilderName}();
                childBuilder = configures[i](childBuilder);
                newItems[i] = childBuilder.Build();
            }}"
				: "";
			
			var addItemsCode = isArray
				? $@"if ({fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null)
            {{
                var existingArray = {fieldName}.Value.Object;
                {collectionVarName} = new {elementTypeName}[existingArray.Length + newItems.Length];
                System.Array.Copy(existingArray, 0, {collectionVarName}, 0, existingArray.Length);
                System.Array.Copy(newItems, 0, {collectionVarName}, existingArray.Length, newItems.Length);
            }}
            else
            {{
                {collectionVarName} = newItems;
            }}"
				: $@"if ({fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null)
            {{
                {collectionVarName} = {fieldName}.Value.Object;
            }}
            else
            {{
                {collectionVarName} = new {typedSymbol.TypeFullName}();
            }}
            
            foreach (var configure in configures)
            {{
                var childBuilder = new {childBuilderName}();
                childBuilder = configure(childBuilder);
                {collectionVarName}.Add(childBuilder.Build());
            }}";
			
			return $@"public {_builder.FullName} {methodName}(params System.Func<{childBuilderName}, {childBuilderName}>[] configures)
        {{
            {buildItemsCode}{(isArray ? "\n            \n            " : "")}{collectionTypeName} {collectionVarName};
            {addItemsCode}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>({collectionVarName});
            return this;
        }}";
		}
		
		// For interface collection types, use List<T>
		return $@"public {_builder.FullName} {methodName}(params System.Func<{childBuilderName}, {childBuilderName}>[] configures)
        {{
            var list = {fieldName} != null && {fieldName}.HasValue && {fieldName}.Value.Object != null
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