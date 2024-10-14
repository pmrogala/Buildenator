using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;
using System;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(EntityWithPrivateConstructor))]
public partial class EntityWithPrivateConstructorBuilder
{
    public EntityWithPrivateConstructor Build()
    {
        throw new InvalidOperationException("It is a test!");
    }
}
