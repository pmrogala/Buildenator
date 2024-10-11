using Buildenator.CodeAnalysis;
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

    IReadOnlyList<ITypedSymbol> GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch();
    IReadOnlyList<ITypedSymbol> GetAllUniqueSettablePropertiesAndParameters();
}