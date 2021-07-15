using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntities;
using Buildenator.IntegrationTests.Source.Fixtures;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(SettableEntityWithoutConstructor))]
    [AutoFixtureConfiguration(nameof(CustomFixtureInheritedFromExternal))]
    public partial class SettableEntityWithoutConstructorBuilder
    {
    }
}
