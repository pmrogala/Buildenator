using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(Entity), defaultStaticCreator: false)]
    public partial class EntityBuilder
    {
    }
}
