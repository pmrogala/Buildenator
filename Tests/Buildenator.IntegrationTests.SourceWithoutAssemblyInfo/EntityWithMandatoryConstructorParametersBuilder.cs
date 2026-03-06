using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.SourceWithoutAssemblyInfo
{
    [MakeBuilder(typeof(EntityWithMandatoryConstructorParameters), builderConstructorMandatoryParameters: true)]
    public partial class EntityWithMandatoryConstructorParametersBuilder
    {
    }
}
