using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(ReadOnlyEntityWithConstructor), nullableStrategy: NullableStrategy.Enabled, generateMethodsForUnreachableProperties: true)]
    [AutoFixtureConfiguration()]
    public partial class ReadOnlyEntityWithConstructorBuilder
    {
    }
}
