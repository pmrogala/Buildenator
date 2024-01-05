using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.Abstraction.Moq;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    [MoqConfiguration(MockingInterfacesStrategy.None)]
    // For Nullable strategy enabled, the fixture interface strategy cannot be None.
    [AutoFixtureConfiguration()]
    public partial class GrandchildEntityNoMoqBuilder
    {
    }
}
