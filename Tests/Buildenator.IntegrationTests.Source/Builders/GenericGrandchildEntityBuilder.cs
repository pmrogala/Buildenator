using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(GrandchildEntity<,>), defaultStaticCreator: true)]
    [AutoFixtureConfiguration()]
    public partial class GenericGrandchildEntityBuilder<T, TK>
    {
    }
}
