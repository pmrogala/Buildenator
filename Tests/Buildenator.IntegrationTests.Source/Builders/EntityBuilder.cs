using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(Entity), defaultStaticCreator: false, implicitCast: true)]
    public partial class EntityBuilder
    {
    }
}
