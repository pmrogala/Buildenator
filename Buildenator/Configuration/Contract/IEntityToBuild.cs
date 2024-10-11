using Buildenator.CodeAnalysis;
using Buildenator.Generators;
using System.Collections.Generic;

namespace Buildenator.Configuration.Contract
{
    internal interface IEntityToBuild : IAdditionalNamespacesProvider
    {
        EntityToBuild.Constructor? ConstructorToBuild { get; }
        string FullName { get; }
        string FullNameWithConstraints { get; }
        string Name { get; }
        IReadOnlyList<TypedSymbol> SettableProperties { get; }

        IReadOnlyList<ITypedSymbol> GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch();
        IReadOnlyList<ITypedSymbol> GetAllUniqueSettablePropertiesAndParameters();
    }
}