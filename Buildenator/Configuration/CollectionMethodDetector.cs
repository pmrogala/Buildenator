using Buildenator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Buildenator.Configuration;

internal static class CollectionMethodDetector
{
    /// <summary>
    /// Factory method that creates CollectionMetadata for a given type symbol.
    /// Returns ConcreteDictionaryMetadata or InterfaceDictionaryMetadata for dictionary types,
    /// ConcreteCollectionMetadata for concrete collection types,
    /// InterfaceCollectionMetadata for interface collection types,
    /// or null if the type is not a collection.
    /// </summary>
    public static CollectionMetadata? CreateCollectionMetadata(ITypeSymbol propertyType)
    {
        // Check for dictionary types FIRST (before collection check)
        var dictionaryMetadata = CreateDictionaryMetadata(propertyType);
        if (dictionaryMetadata != null)
        {
            return dictionaryMetadata;
        }
        
        // Check for concrete collection types (before interface check)
        if (IsConcreteCollectionProperty(propertyType))
        {
            var elementType = GetCollectionElementType(propertyType);
            if (elementType != null)
            {
                return new ConcreteCollectionMetadata(elementType);
            }
        }
        
        // Then check for interface collection types
        if (IsInterfaceCollectionProperty(propertyType))
        {
            var elementType = GetCollectionElementType(propertyType);
            if (elementType != null)
            {
                return new InterfaceCollectionMetadata(elementType);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Creates dictionary metadata for the given type if it's a dictionary type.
    /// Returns ConcreteDictionaryMetadata for Dictionary&lt;K,V&gt;, 
    /// InterfaceDictionaryMetadata for IDictionary&lt;K,V&gt; and IReadOnlyDictionary&lt;K,V&gt;,
    /// or null if not a dictionary type.
    /// </summary>
    private static CollectionMetadata? CreateDictionaryMetadata(ITypeSymbol propertyType)
    {
        if (propertyType is not INamedTypeSymbol namedType || !namedType.IsGenericType)
        {
            return null;
        }
        
        var constructedFrom = namedType.ConstructedFrom;
        if (constructedFrom == null)
        {
            return null;
        }
        
        var typeName = constructedFrom.ToDisplayString();
        
        // Check for concrete Dictionary<TKey, TValue>
        if (typeName == "System.Collections.Generic.Dictionary<TKey, TValue>")
        {
            if (namedType.TypeArguments.Length >= 2)
            {
                var keyType = namedType.TypeArguments[0];
                var valueType = namedType.TypeArguments[1];
                var elementType = GetCollectionElementType(propertyType);
                if (elementType != null)
                {
                    return new ConcreteDictionaryMetadata(keyType, valueType, elementType);
                }
            }
        }
        
        // Check for IDictionary<TKey, TValue> or IReadOnlyDictionary<TKey, TValue>
        if (typeName == "System.Collections.Generic.IDictionary<TKey, TValue>" ||
            typeName == "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>")
        {
            if (namedType.TypeArguments.Length >= 2)
            {
                var keyType = namedType.TypeArguments[0];
                var valueType = namedType.TypeArguments[1];
                var elementType = GetCollectionElementType(propertyType);
                if (elementType != null)
                {
                    return new InterfaceDictionaryMetadata(keyType, valueType, elementType);
                }
            }
        }
        
        // Check if the type implements IDictionary<K,V> (for derived dictionary types)
        var dictionaryInterface = namedType.AllInterfaces.FirstOrDefault(i =>
            i.IsGenericType &&
            i.ConstructedFrom != null &&
            i.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.IDictionary<TKey, TValue>");
            
        if (dictionaryInterface != null && dictionaryInterface.TypeArguments.Length >= 2)
        {
            var keyType = dictionaryInterface.TypeArguments[0];
            var valueType = dictionaryInterface.TypeArguments[1];
            var elementType = GetCollectionElementType(propertyType);
            if (elementType != null)
            {
                // If it's a concrete class, return ConcreteDictionaryMetadata
                if (propertyType.TypeKind == TypeKind.Class)
                {
                    return new ConcreteDictionaryMetadata(keyType, valueType, elementType);
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Checks if the type is an interface collection type (implements IEnumerable<T> and is an interface).
    /// Excludes concrete types like List<T>, only returns true for interface types.
    /// </summary>
    private static bool IsInterfaceCollectionProperty(ITypeSymbol propertyType)
    {
        // Only process interface types (also excludes string which is a concrete type)
        if (propertyType.TypeKind != TypeKind.Interface)
        {
            return false;
        }

        // Check if the type implements IEnumerable<T>
        if (propertyType is INamedTypeSymbol namedType)
        {
            // Check if it's IEnumerable<T> itself
            if (namedType.IsGenericType && 
                namedType.ConstructedFrom?.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return true;
            }

            // Check if it implements IEnumerable<T>
            return namedType.AllInterfaces.Any(i => 
                i.IsGenericType && 
                i.ConstructedFrom?.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
        }

        return false;
    }
    
    /// <summary>
    /// Checks if the type is a concrete collection type (implements ICollection<T> and is a class).
    /// Only returns true for concrete classes like List<T>, HashSet<T>, etc.
    /// </summary>
    private static bool IsConcreteCollectionProperty(ITypeSymbol propertyType)
    {
        // Only process class types (excludes interfaces and string)
        if (propertyType.TypeKind != TypeKind.Class)
        {
            return false;
        }
        
        // Exclude string type explicitly
        if (propertyType.SpecialType == SpecialType.System_String)
        {
            return false;
        }

        // Check if the type implements ICollection<T>
        if (propertyType is INamedTypeSymbol namedType)
        {
            // Check interfaces for ICollection<T>
            var collectionInterface = namedType.AllInterfaces.FirstOrDefault(i => 
                i.IsGenericType && 
                i.ConstructedFrom != null &&
                i.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.ICollection<T>");

            return collectionInterface != null;
        }

        return false;
    }
    
    /// <summary>
    /// Gets the element type of the collection (T in IEnumerable<T>).
    /// Returns null if the property is not a collection.
    /// </summary>
    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol propertyType)
    {
        // Exclude string type
        if (propertyType.SpecialType == SpecialType.System_String)
        {
            return null;
        }

        if (propertyType is INamedTypeSymbol namedType)
        {
            // Check if it's IEnumerable<T> itself
            if (namedType.IsGenericType && 
                namedType.ConstructedFrom?.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return namedType.TypeArguments.FirstOrDefault();
            }

            // Check interfaces for IEnumerable<T>
            var enumerableInterface = namedType.AllInterfaces.FirstOrDefault(i => 
                i.IsGenericType && 
                i.ConstructedFrom?.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);

            return enumerableInterface?.TypeArguments.FirstOrDefault();
        }

        return null;
    }
}
