using Buildenator.CodeAnalysis;
using Buildenator.Generators;
using System.Collections.Generic;

namespace Buildenator.Configuration.Contract
{
    internal interface IEntityToBuild : IAdditionalNamespacesProvider
    {
        IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }
        string ContainingNamespace { get; }
        string FullName { get; }
        string FullNameWithConstraints { get; }
        string Name { get; }
        IEnumerable<TypedSymbol> SettableProperties { get; }

        IReadOnlyList<ITypedSymbol> GetAllUniqueNotSettablePropertiesWithoutConstructorsParametersMatch();
        IReadOnlyList<ITypedSymbol> GetAllUniqueSettablePropertiesAndParameters();
    }
}