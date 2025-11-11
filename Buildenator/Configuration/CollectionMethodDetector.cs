using Microsoft.CodeAnalysis;
using System.Linq;

namespace Buildenator.Configuration;

internal static class CollectionMethodDetector
{
    /// <summary>
    /// Checks if the type is an interface collection type (implements IEnumerable<T> and is an interface).
    /// Excludes concrete types like List<T>, only returns true for interface types.
    /// </summary>
    public static bool IsInterfaceCollectionProperty(ITypeSymbol propertyType)
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
    /// Gets the element type of the collection (T in IEnumerable<T>).
    /// Returns null if the property is not a collection.
    /// </summary>
    public static ITypeSymbol? GetCollectionElementType(ITypeSymbol propertyType)
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
