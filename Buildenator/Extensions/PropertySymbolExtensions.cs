﻿using Microsoft.CodeAnalysis;

namespace Buildenator.Extensions
{
    internal static class PropertySymbolExtensions
    {
        public static bool IsSetableProperty(this IPropertySymbol x)
            => x.SetMethod is not null && x.SetMethod.DeclaredAccessibility == Accessibility.Public && x.CanBeReferencedByName;
    }
}