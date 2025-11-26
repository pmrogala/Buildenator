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
    bool IsPreBuildMethodOverriden { get; }
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
    /// Gets the set of user-defined default value names available in this builder.
    /// Contains names like "DefaultName", "DefaultEmail", etc.
    /// </summary>
    IReadOnlyCollection<string> DefaultValueNames { get; }
}