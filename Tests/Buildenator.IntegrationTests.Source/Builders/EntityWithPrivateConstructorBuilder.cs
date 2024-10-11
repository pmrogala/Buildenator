using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(EntityWithPrivateConstructor))]
public partial class EntityWithPrivateConstructorBuilder
{
    public EntityWithPrivateConstructor Build()
    {
        return default!;
    }
}
