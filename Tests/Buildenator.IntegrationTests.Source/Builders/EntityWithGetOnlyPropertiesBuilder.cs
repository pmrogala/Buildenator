using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(EntityWithGetOnlyProperties), nullableStrategy: NullableStrategy.Enabled)]
[AutoFixtureConfiguration]
public partial class EntityWithGetOnlyPropertiesBuilder
{
}
