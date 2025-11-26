using Buildenator.Abstraction;
using Buildenator.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Buildenator.Configuration.Contract;

internal interface IBuilderProperties
{
    IReadOnlyDictionary<string, List<IMethodSymbol>> BuildingMethods { get; }
    string BuildingMethodsPrefix { get; }
    string ContainingNamespace { get; }
    IReadOnlyDictionary<string, IFieldSymbol> Fields { get; }
    string FullName { get; }
    string Name { get; }
    NullableStrategy NullableStrategy { get; }
    bool GenerateDefaultBuildMethod { get; }
    bool ImplicitCast { get; }
    bool IsPostBuildMethodOverriden { get; }
    bool IsDefaultConstructorOverriden { get; }
    bool ShouldGenerateMethodsForUnreachableProperties { get; }
    Location OriginalLocation { get; }
    bool IsBuildMethodOverriden { get; }
    bool IsBuildManyMethodOverriden { get; }
    IEnumerable<BuildenatorDiagnostic> Diagnostics { get; }
    bool GenerateStaticPropertyForBuilderCreation { get; }
    bool InitializeCollectionsWithEmpty { get; }
    
    /// <summary>
    /// Gets the user-defined default value expression for a property with the given pascal case name, if any.
    /// Looks for static fields or constants named "Default{PropertyPascalName}".
    /// </summary>
    /// <param name="propertyPascalName">The pascal case name of the property (e.g., "Name" for a property named "name").</param>
    /// <returns>The default value name (e.g., "DefaultName") if found, otherwise null.</returns>
    string? GetDefaultValueName(string propertyPascalName);
}