using AutoFixture;
using Buildenator.Abstraction;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(SettableEntityWithConstructor))]
    [FixtureConfiguration(typeof(Fixture))]
    public partial class SettableEntityWithConstructorBuilder
    {
    }
}
