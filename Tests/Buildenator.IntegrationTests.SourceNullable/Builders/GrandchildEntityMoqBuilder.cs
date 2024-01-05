using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    [AutoFixtureWithMoqConfiguration]
    public partial class GrandchildEntityMoqBuilder
    {
    }
}
