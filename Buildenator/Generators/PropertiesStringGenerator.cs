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

		// Create child builder lookup function if UseChildBuilders is enabled
		System.Func<string, string?>? childBuilderLookup = null;
		if (_builder.UseChildBuilders && _entityToBuilderMappings != null)
		{
			childBuilderLookup = elementTypeName => 
				_entityToBuilderMappings.TryGetValue(elementTypeName, out var builderName) ? builderName : null;
		}

		var output = new StringBuilder();

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredField))
		{
            output = output.AppendLine($@"        private {typedSymbol.GenerateLazyFieldType()} {typedSymbol.UnderScoreName}{GenerateFieldInitializer(typedSymbol)};");
		}

		// Generate With methods for properties
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

		// Generate child builder methods if UseChildBuilders is enabled
		if (_builder.UseChildBuilders && _entityToBuilderMappings != null)
		{
			foreach (var typedSymbol in properties.Where(HasChildBuilder).Where(IsNotYetDeclaredChildBuilderMethod))
			{
				output = output.AppendLine($@"

        {GenerateChildBuilderMethodDefinition(typedSymbol)}");
			}
			
			// Generate AddTo methods with Func<ChildBuilder, ChildBuilder> for collections with child builders
			foreach (var typedSymbol in properties.Where(IsNotYetDeclaredChildBuilderAddToMethod))
			{
				var collectionMetadata = typedSymbol.GetCollectionMetadata(childBuilderLookup);
				if (collectionMetadata?.HasChildBuilder == true)
				{
					output = output.AppendLine($@"

        {GenerateChildBuilderAddToMethodDefinition(typedSymbol, collectionMetadata)}");
				}
			}
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

			// Check if any method has a matching params array parameter
			return !methods.Any(method => 
				method.Parameters.Length == 1 && 
				method.Parameters[0].IsParams &&
				method.Parameters[0].Type is IArrayTypeSymbol arrayType &&
				arrayType.ElementType.Name == collectionMetadata.ElementTypeName);
		}

		bool IsCollectionProperty(ITypedSymbol x) => x.GetCollectionMetadata() != null && !x.IsMockable();
		
		bool HasChildBuilder(ITypedSymbol x) => !x.IsMockable() && TryGetChildBuilderName(x, out _);
		
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
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(({typedSymbol.TypeFullName})list);
            return this;
        }}";
	}

	private string CreateAddToMethodName(ITypedSymbol property) => $"AddTo{property.SymbolPascalName}";

	/// <summary>
	/// Tries to get the builder name for a property's type.
	/// Returns true if a child builder exists for the property's type.
	/// </summary>
	private bool TryGetChildBuilderName(ITypedSymbol typedSymbol, out string childBuilderName)
	{
		childBuilderName = string.Empty;
		if (_entityToBuilderMappings == null)
			return false;
		
		var typeFullName = typedSymbol.TypeFullName;
		return _entityToBuilderMappings.TryGetValue(typeFullName, out childBuilderName!);
	}

	/// <summary>
	/// Generates a method that accepts Func&lt;ChildBuilder, ChildBuilder&gt; for configuring child entities.
	/// </summary>
	private string GenerateChildBuilderMethodDefinition(ITypedSymbol typedSymbol)
	{
		if (!TryGetChildBuilderName(typedSymbol, out var childBuilderName))
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
	private string GenerateChildBuilderAddToMethodDefinition(ITypedSymbol typedSymbol, CollectionMetadata collectionMetadata)
	{
		var childBuilderName = collectionMetadata.ChildBuilderName!;
		var elementTypeName = collectionMetadata.ElementTypeDisplayName;
		var methodName = CreateAddToMethodName(typedSymbol);
		var fieldName = typedSymbol.UnderScoreName;
		
		// For concrete collection types
		if (collectionMetadata is ConcreteCollectionMetadata)
		{
			return $@"public {_builder.FullName} {methodName}(params System.Func<{childBuilderName}, {childBuilderName}>[] configures)
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
            
            foreach (var configure in configures)
            {{
                var childBuilder = new {childBuilderName}();
                childBuilder = configure(childBuilder);
                collection.Add(childBuilder.Build());
            }}
            
            {fieldName} = new {DefaultConstants.NullBox}<{typedSymbol.TypeFullName}>(collection);
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