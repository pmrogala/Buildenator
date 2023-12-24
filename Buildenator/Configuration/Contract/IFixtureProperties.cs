using Buildenator.Abstraction;
using Buildenator.Generators;

namespace Buildenator.Configuration.Contract;

internal interface IFixtureProperties : IAdditionalNamespacesProvider
{
    string? AdditionalConfiguration { get; }
    string? ConstructorParameters { get; }
    string CreateSingleFormat { get; }
    string Name { get; }
    FixtureInterfacesStrategy Strategy { get; }

    string GenerateAdditionalConfiguration();
    bool NeedsAdditionalConfiguration();
}