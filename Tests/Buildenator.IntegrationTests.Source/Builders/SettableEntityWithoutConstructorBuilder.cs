using Buildenator.Abstraction;
using Buildenator.IntegrationTests.Source.Fixtures;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(SettableEntityWithoutConstructor))]
    [FixtureConfiguration(typeof(CustomFixtureInheritedFromExternal), "AutoFixture")]
    public partial class SettableEntityWithoutConstructorBuilder
    {
    }
}
