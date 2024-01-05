using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(Entity), implicitCast: true)]
    public partial class ImplicitCastBuilder
    {
    }
}