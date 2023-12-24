using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Buildenator.Configuration.Contract;

internal interface IBuilderProperties
{
    IReadOnlyDictionary<string, IMethodSymbol> BuildingMethods { get; }
    string BuildingMethodsPrefix { get; }
    string ContainingNamespace { get; }
    IReadOnlyDictionary<string, IFieldSymbol> Fields { get; }
    string FullName { get; }
    string Name { get; }
    NullableStrategy NullableStrategy { get; }
    bool StaticCreator { get; }
    bool ImplicitCast { get; }
    bool IsPostBuildMethodOverriden { get; }
    bool IsDefaultConstructorOverriden { get; }
    bool ShouldGenerateMethodsForUnreachableProperties { get; }
}