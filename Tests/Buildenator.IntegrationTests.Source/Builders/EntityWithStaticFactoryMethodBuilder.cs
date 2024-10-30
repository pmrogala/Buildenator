using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(Entity), staticFactoryMethodName: nameof(SharedEntities.Entity.CreateEntity))]
public partial class EntityWithStaticFactoryMethodBuilder
{

}