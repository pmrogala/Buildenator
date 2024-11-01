using Buildenator.Abstraction;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(EntityWithNoNamespace), generateStaticPropertyForBuilderCreation: false, staticFactoryMethodName: nameof(EntityWithNoNamespace.CreateEntity))]
public partial class EntityWithNoNamespaceBuilder
{

}