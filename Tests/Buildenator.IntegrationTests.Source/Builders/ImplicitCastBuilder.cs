using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(Entity), implicitCast: true)]
    public partial class ImplicitCastBuilder
    {
    }
}