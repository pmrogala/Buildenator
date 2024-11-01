using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(Entity), generateStaticPropertyForBuilderCreation: false, staticFactoryMethodName: nameof(Entity.CreateEntity))]
public partial class EntityWithStaticFactoryMethodBuilder
{

}