using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(SettableEntityWithoutConstructor))]
    [AutoFixtureConfiguration("Buildenator.IntegrationTests.SourceNullable.Fixtures.CustomFixtureInheritedFromExternal")]
    public partial class SettableEntityWithoutConstructorBuilder
    {
    }
}
