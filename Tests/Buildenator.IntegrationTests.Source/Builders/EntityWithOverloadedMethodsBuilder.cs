using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(EntityWithOverloadedMethods))]
public partial class EntityWithOverloadedMethodsBuilder
{
    public static EntityWithOverloadedMethodsBuilder Default(int aValue = 1) => new EntityWithOverloadedMethodsBuilder()
        .WithAValue(aValue);

    public EntityWithOverloadedMethodsBuilder WithValue(int v) => this;

    public EntityWithOverloadedMethodsBuilder WithValue(string v) => this;
}
