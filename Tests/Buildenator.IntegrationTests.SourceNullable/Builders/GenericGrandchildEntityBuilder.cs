using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(GrandchildEntity<,>))]
    [AutoFixtureConfiguration]
    public partial class GenericGrandchildEntityBuilder<T, TK>
    {
    }
}
