using Buildenator.Abstraction;
using Buildenator.Abstraction.Moq;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(ChildEntity))]
    [MoqConfiguration(MockingInterfacesStrategy.All)]
    public partial class ChildEntityBuilder
    {
    }
}
