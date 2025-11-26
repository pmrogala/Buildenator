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
        IBuilderProperties builderProperties)
    {
            var hasAnyBody = false;
            var parameters = entity.AllUniqueSettablePropertiesAndParameters;

            var output = new StringBuilder();
        output = output.AppendLine($@"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public {builderName}()
        {{");
            foreach (var typedSymbol in parameters.Where(a => a.NeedsFieldInit()))
            {
                output = output.AppendLine($@"            {typedSymbol.GenerateFieldInitialization()}");
                hasAnyBody = true;
            }

            // Generate empty collection initializations if the option is enabled
            if (builderProperties.InitializeCollectionsWithEmpty)
            {
                foreach (var typedSymbol in parameters.Where(a => !a.NeedsFieldInit() && !a.IsMockable()))
                {
                    var collectionMetadata = typedSymbol.GetCollectionMetadata();
                    if (collectionMetadata != null)
                    {
                        var initCode = GenerateEmptyCollectionInitialization(typedSymbol, collectionMetadata);
                        if (!string.IsNullOrEmpty(initCode))
                        {
                            output = output.AppendLine($@"            {initCode}");
                            hasAnyBody = true;
                        }
                    }
                }
            }

            if (fixtureConfiguration is not null && fixtureConfiguration.NeedsAdditionalConfiguration())
            {
                output = output.AppendLine($@"            {fixtureConfiguration.GenerateAdditionalConfiguration()};");
                hasAnyBody = true;
            }

            output = output.AppendLine($@"
        }}");

            return hasAnyBody ? output.ToString() : string.Empty;
        }

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
        
        // For concrete collection types, create new instance
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