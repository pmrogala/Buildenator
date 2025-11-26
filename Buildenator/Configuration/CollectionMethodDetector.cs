using Buildenator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Buildenator.Configuration;

internal static class CollectionMethodDetector
{
    /// <summary>
    /// Factory method that creates CollectionMetadata for a given type symbol.
    /// Returns ConcreteCollectionMetadata for concrete collection types,
    /// InterfaceCollectionMetadata for interface collection types,
    /// or null if the type is not a collection.
    /// Dictionary types are explicitly excluded as they require special handling.
    /// </summary>
    public static CollectionMetadata? CreateCollectionMetadata(ITypeSymbol propertyType)
    {
        // Exclude dictionary types - they should not be treated as regular collections
        if (IsDictionaryType(propertyType))
        {
            return null;
        }
        
        // Check for concrete collection types FIRST (before interface check)
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
    /// Checks if the type is a dictionary type (Dictionary, IDictionary, or IReadOnlyDictionary).
    /// Dictionary types should not be treated as regular collections because:
    /// 1. They require key-value pair handling, not single element handling
    /// 2. Their Add methods take two parameters (key, value), not a single item
    /// 3. List&lt;KeyValuePair&lt;K,V&gt;&gt; is not assignable to dictionary interfaces
    /// </summary>
    private static bool IsDictionaryType(ITypeSymbol propertyType)
    {
        if (propertyType is not INamedTypeSymbol namedType || !namedType.IsGenericType)
        {
            return false;
        }
        
        var constructedFrom = namedType.ConstructedFrom;
        if (constructedFrom == null)
        {
            return false;
        }
        
        var typeName = constructedFrom.ToDisplayString();
        
        // Check for common dictionary types
        if (typeName == "System.Collections.Generic.Dictionary<TKey, TValue>" ||
            typeName == "System.Collections.Generic.IDictionary<TKey, TValue>" ||
            typeName == "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>")
        {
            return true;
        }
        
        // Also check if the type implements IDictionary<K,V> (for concrete dictionary types)
        return namedType.AllInterfaces.Any(i =>
            i.IsGenericType &&
            i.ConstructedFrom != null &&
            i.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.IDictionary<TKey, TValue>");
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
