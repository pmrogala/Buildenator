using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.Abstraction.Moq;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    [MoqConfiguration(MockingInterfacesStrategy.None)]
    [AutoFixtureConfiguration(strategy: FixtureInterfacesStrategy.None)]
    public partial class GrandchildEntityNoMoqBuilder
    {
    }
}
