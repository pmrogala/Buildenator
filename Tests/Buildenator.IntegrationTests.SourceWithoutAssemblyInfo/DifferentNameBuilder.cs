using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.SourceWithoutAssemblyInfo
{
    [MakeBuilder(typeof(Entity))]
    public partial class DifferentNameBuilder
    {
    }
}
