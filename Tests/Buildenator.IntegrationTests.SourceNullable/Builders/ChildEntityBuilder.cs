using Buildenator.Abstraction;
using Buildenator.Abstraction.Moq;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(ChildEntity))]
    [MoqConfiguration(MockingInterfacesStrategy.All)]
    public partial class ChildEntityBuilder
    {
    }
}
