using Buildenator.CodeAnalysis;
using Buildenator.Diagnostics;
using Buildenator.Generators;
using System.Collections.Generic;

namespace Buildenator.Configuration.Contract;

internal interface IEntityToBuild : IAdditionalNamespacesProvider
{
    string FullName { get; }
    string FullNameWithConstraints { get; }
    string Name { get; }
    IReadOnlyList<TypedSymbol> SettableProperties { get; }
    IReadOnlyList<TypedSymbol> ReadOnlyProperties { get; }
    EntityToBuild.Constructor? ConstructorToBuild { get; }
    IEnumerable<BuildenatorDiagnostic> Diagnostics { get; }

    string GenerateDefaultBuildEntityString(IEnumerable<ITypedSymbol> parameters, IEnumerable<ITypedSymbol> properties);
    string GenerateStaticBuildsCode();
    IReadOnlyList<ITypedSymbol> GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch();
    IReadOnlyList<ITypedSymbol> GetAllUniqueSettablePropertiesAndParameters();
    (IReadOnlyList<ITypedSymbol> Parameters, IReadOnlyList<ITypedSymbol> Properties) GetParametersAndProperties();
}