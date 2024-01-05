using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(PostBuildEntity), defaultStaticCreator: false, generateMethodsForUnreachableProperties: true)]
    public partial class PostBuildEntityBuilder
    {
        public void PostBuild(PostBuildEntity buildResult)
        {
            buildResult.Entry = -1;
        }
    }
}
