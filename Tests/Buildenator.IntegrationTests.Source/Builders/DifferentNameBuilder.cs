using Buildenator.Abstraction;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(Entity))]
    public partial class DifferentNameBuilder
    {
    }

    [MakeBuilder(typeof(SettableEntityWithoutConstructor))]
    public partial class SettableEntityWithoutConstructorBuilder
    {
    }
    [MakeBuilder(typeof(SettableEntityWithConstructor))]
    public partial class SettableEntityWithConstructorBuilder
    {
    }
}
