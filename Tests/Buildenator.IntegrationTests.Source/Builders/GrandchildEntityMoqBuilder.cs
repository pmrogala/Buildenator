using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    [AutoFixtureWithMoqConfiguration()]
    public partial class GrandchildEntityMoqBuilder
    {
    }
}
