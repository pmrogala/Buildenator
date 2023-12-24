using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(PostBuildEntity), defaultStaticCreator: false, generateMethodsForUnreachableProperties: true)]
public partial class PostBuildEntityBuilder
{
    public void PostBuild(PostBuildEntity buildResult)
    {
            buildResult.Entry = -1;
        }
}