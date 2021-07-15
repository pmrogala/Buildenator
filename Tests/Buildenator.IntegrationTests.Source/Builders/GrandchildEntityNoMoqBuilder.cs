using Buildenator.Abstraction;
using Buildenator.Abstraction.Moq;
using Buildenator.IntegrationTests.Source.Fixtures;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    [MoqConfiguration(MockingInterfacesStrategy.None)]
    [FixtureConfiguration(typeof(CustomFixture), FixtureInterfacesStrategy.None)]
    public partial class GrandchildEntityNoMoqBuilder
    {
    }
}
