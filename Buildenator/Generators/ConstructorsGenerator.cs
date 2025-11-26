using Buildenator.Configuration.Contract;
using System.Linq;
using System.Text;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration;

namespace Buildenator.Generators;

internal static class ConstructorsGenerator
{
    internal static string GenerateConstructor(
        string builderName,
        IEntityToBuild entity,
        IFixtureProperties? fixtureConfiguration,
        bool initializeCollectionsWithEmpty,
        IBuilderProperties builder)
    {
            var parameters = entity.AllUniqueSettablePropertiesAndParameters;

            var output = new StringBuilder();
        output = output.AppendLine($@"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public {builderName}()
        {{");
            foreach (var typedSymbol in parameters.Where(a => a.NeedsFieldInit()))
            {
                output = output.AppendLine($@"            {typedSymbol.GenerateFieldInitialization()}");
            }

            // Generate empty collection initializations if the option is enabled
            if (initializeCollectionsWithEmpty)
            {
                foreach (var typedSymbol in parameters.Where(ts => ShouldInitializeCollectionField(ts, builder)))
                {
                    var collectionMetadata = typedSymbol.GetCollectionMetadata();
                    if (collectionMetadata != null)
                    {
                        var initCode = GenerateEmptyCollectionInitialization(typedSymbol, collectionMetadata);
                        if (!string.IsNullOrEmpty(initCode))
                        {
                            output = output.AppendLine($@"            {initCode}");
                        }
                    }
                }
            }

            if (fixtureConfiguration is not null && fixtureConfiguration.NeedsAdditionalConfiguration())
            {
                output = output.AppendLine($@"            {fixtureConfiguration.GenerateAdditionalConfiguration()};");
            }

            output = output.AppendLine($@"            {DefaultConstants.PreBuildMethodName}();");

            output = output.AppendLine($@"
        }}");

            return output.ToString();
        }

    /// <summary>
    /// Determines if a typed symbol should be considered for empty collection initialization.
    /// This method is called for collection-type properties only (caller filters via GetCollectionMetadata).
    /// Excludes fields that already have initialization (NeedsFieldInit), are mockable, or have a user-defined default value.
    /// </summary>
    private static bool ShouldInitializeCollectionField(ITypedSymbol typedSymbol, IBuilderProperties builder)
    {
        // Exclude fields that have auto-initialization or are mockable
        if (typedSymbol.NeedsFieldInit() || typedSymbol.IsMockable())
            return false;
        
        // Exclude fields that have a user-defined default value (e.g., Default{PropertyName})
        // to avoid redundant initialization
        var defaultValueName = builder.GetDefaultValueName(typedSymbol.SymbolPascalName);
        if (defaultValueName != null)
            return false;
        
        return true;
    }

    /// <summary>
    /// Generates code to initialize a collection field with an empty collection.
    /// For interface types, creates a concrete implementation (e.g., List&lt;T&gt; for IEnumerable&lt;T&gt;).
    /// For concrete types, creates a new instance using parameterless constructor.
    /// Note: Concrete collection types must have a parameterless constructor for this to work.
    /// Standard .NET collection types like List&lt;T&gt;, HashSet&lt;T&gt;, Dictionary&lt;K,V&gt; all support this.
    /// </summary>
    private static string GenerateEmptyCollectionInitialization(ITypedSymbol typedSymbol, CollectionMetadata collectionMetadata)
    {
        var fieldName = typedSymbol.UnderScoreName;
        var typeFullName = typedSymbol.TypeFullName;
        
        // For concrete dictionary types, create new instance
        if (collectionMetadata is ConcreteDictionaryMetadata)
        {
            return $"{fieldName} = new {DefaultConstants.NullBox}<{typeFullName}>(new {typeFullName}());";
        }
        
        // For interface dictionary types, create a Dictionary<K,V>
        if (collectionMetadata is InterfaceDictionaryMetadata dictMetadata)
        {
            var dictionaryType = $"System.Collections.Generic.Dictionary<{dictMetadata.KeyType.ToDisplayString()}, {dictMetadata.ValueType.ToDisplayString()}>";
            return $"{fieldName} = new {DefaultConstants.NullBox}<{typeFullName}>(new {dictionaryType}());";
        }
        
        // For concrete collection types, create new instance.
        // Note: This assumes the concrete type has a parameterless constructor.
        // Standard .NET collections (List<T>, HashSet<T>, Collection<T>, etc.) all support this.
        if (collectionMetadata is ConcreteCollectionMetadata)
        {
            return $"{fieldName} = new {DefaultConstants.NullBox}<{typeFullName}>(new {typeFullName}());";
        }
        
        // For interface collection types, create a List<T>
        if (collectionMetadata is InterfaceCollectionMetadata)
        {
            var elementTypeName = collectionMetadata.ElementType.ToDisplayString();
            var listType = $"System.Collections.Generic.List<{elementTypeName}>";
            return $"{fieldName} = new {DefaultConstants.NullBox}<{typeFullName}>(new {listType}());";
        }
        
        return string.Empty;
    }
}