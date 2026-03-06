using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.SourceWithoutAssemblyInfo
{
    [MakeBuilder(
        typeof(EntityWithMandatoryConstructorParameters),
        builderConstructorMandatoryParameters: true,
        initializeCollectionsWithEmpty: true)]
    public partial class EntityWithMandatoryConstructorParametersBuilder
    {
    }
}
