using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator.Extensions
{
    internal static class SymbolExtensions
    {
        public static string PascalCaseName(this ISymbol symbol)
            => $"{symbol.Name.Substring(0, 1).ToUpperInvariant()}{symbol.Name.Substring(1)}";
        public static string CamelCaseName(this ISymbol symbol)
            => $"{symbol.Name.Substring(0, 1).ToLowerInvariant()}{symbol.Name.Substring(1)}";
        public static string UnderScoreName(this ISymbol symbol)
            => $"_{symbol.CamelCaseName()}";

        public static (IEnumerable<IPropertySymbol> Settable, IEnumerable<IPropertySymbol> ReadOnly)
            DividePublicPropertiesBySetability(this INamedTypeSymbol entityToBuildSymbol)
        {
            var (setProperties, unsetProperties) = entityToBuildSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(a => a.GetMethod is not null && a.GetMethod.DeclaredAccessibility != Accessibility.Private && a.GetMethod.DeclaredAccessibility != Accessibility.Protected)
                .Split(a => a.IsSettableProperty())
                .ToLists();

            var setPropertyNames = new HashSet<string>(setProperties.Select(x => x.Name));
            var unsetPropertyNames = new HashSet<string>(unsetProperties.Select(x => x.Name));
            var baseType = entityToBuildSymbol.BaseType;
            while (baseType != null)
            {
                var newProperties = baseType.GetMembers().OfType<IPropertySymbol>().Split(a => a.IsSettableProperty());
                TakeNotCoverProperties(ref setProperties, setPropertyNames, newProperties.Item1);
                TakeNotCoverProperties(ref unsetProperties, unsetPropertyNames, newProperties.Item2);

                baseType = baseType.BaseType;
            }

            return (setProperties, unsetProperties);

            static void TakeNotCoverProperties(
                ref List<IPropertySymbol> properties, ISet<string> propertyNames, IEnumerable<IPropertySymbol> newProperties)
            {
                var newSetProperties = newProperties.Where(x => !propertyNames.Contains(x.Name)).ToList();

#pragma warning disable RS1024 // Symbols should be compared for equality
                properties = properties.Union(newSetProperties).ToList();
#pragma warning restore RS1024 // Symbols should be compared for equality
                propertyNames.UnionWith(newSetProperties.Select(x => x.Name));
            }
        }
    }
}