using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(Entity), defaultStaticCreator: false)]
public partial class EntityBuilder
{

}
