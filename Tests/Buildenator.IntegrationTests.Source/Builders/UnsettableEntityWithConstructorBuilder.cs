using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(UnsettableEntityWithConstructor), nullableStrategy: NullableStrategy.Enabled, generateMethodsForUnrechableProperties: true)]
    [AutoFixtureConfiguration()]
    public partial class UnsettableEntityWithConstructorBuilder
    {
    }
}
