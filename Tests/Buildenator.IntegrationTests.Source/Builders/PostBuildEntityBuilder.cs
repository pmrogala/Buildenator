using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Builders
{
    [MakeBuilder(typeof(PostBuildEntity), defaultStaticCreator: false, generateMethodsForUnrechableProperties: true)]
    public partial class PostBuildEntityBuilder
    {
        public void PostBuild(PostBuildEntity buildResult)
        {
            buildResult.Entry = -1;
        }
    }
}
