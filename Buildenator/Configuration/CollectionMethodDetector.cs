using Buildenator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Buildenator.Configuration;

internal static class CollectionMethodDetector
{
    /// <summary>
    /// Checks if the entity has an Add method for the given property.
    /// The method name should be "Add{PropertyName}" (e.g., AddItem for Items property).
    /// </summary>
    public static bool HasAddMethodForProperty(INamedTypeSymbol entitySymbol, ITypedSymbol property)
    {
        var propertyName = property.SymbolPascalName;
        
        // Look for a method named "Add{singularPropertyName}"
        // For example, for "Items" property, look for "AddItem" method
        var singularPropertyName = GetSingularPropertyName(propertyName);
        var expectedMethodName = $"Add{singularPropertyName}";
        
        var addMethod = entitySymbol.GetMembers(expectedMethodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => 
                m.DeclaredAccessibility == Accessibility.Public &&
                !m.IsStatic &&
                m.Parameters.Length == 1);
        
        return addMethod != null;
    }
    
    /// <summary>
    /// Gets the singular form of a property name by removing 's' suffix if present.
    /// For example: "Items" -> "Item", "Children" -> "Child" (simplified).
    /// </summary>
    private static string GetSingularPropertyName(string propertyName)
    {
        // Simple heuristic: remove trailing 's' if present
        if (propertyName.Length > 1 && propertyName.EndsWith("s") && !propertyName.EndsWith("ss"))
        {
            return propertyName.Substring(0, propertyName.Length - 1);
        }
        
        return propertyName;
    }
    
    /// <summary>
    /// Gets the parameter type of the Add method for the property.
    /// Returns null if no Add method exists.
    /// </summary>
    public static ITypeSymbol? GetAddMethodParameterType(INamedTypeSymbol entitySymbol, ITypedSymbol property)
    {
        var propertyName = property.SymbolPascalName;
        var singularPropertyName = GetSingularPropertyName(propertyName);
        var expectedMethodName = $"Add{singularPropertyName}";
        
        var addMethod = entitySymbol.GetMembers(expectedMethodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => 
                m.DeclaredAccessibility == Accessibility.Public &&
                !m.IsStatic &&
                m.Parameters.Length == 1);
        
        return addMethod?.Parameters[0].Type;
    }
}
