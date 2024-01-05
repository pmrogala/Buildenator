using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(SettableEntityWithConstructor), nullableStrategy: NullableStrategy.Enabled)]
    [AutoFixtureConfiguration]
    public partial class SettableEntityWithConstructorBuilder
    {
    }
}
