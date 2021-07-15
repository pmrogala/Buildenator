using Buildenator.Abstraction;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    [FixtureConfiguration(typeof(AutoFixture.Fixture))]
    public partial class GrandchildEntityBuilder
    {
    }
}
