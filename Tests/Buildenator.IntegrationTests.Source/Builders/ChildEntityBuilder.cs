using Buildenator.Abstraction;
using Buildenator.Abstraction.Moq;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(ChildEntity))]
[MoqConfiguration(MockingInterfacesStrategy.All)]
public partial class ChildEntityBuilder
{
}