using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(EntityWithImplicitConstructor))]
public partial class EntityWithDefaultConstructorBuilder
{
}