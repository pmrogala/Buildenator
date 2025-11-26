using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(PreBuildEntity), generateDefaultBuildMethod: false, generateStaticPropertyForBuilderCreation: true)]
public partial class PreBuildEntityBuilder
{
    private int _preBuildCalled = 0;

    public void PreBuild()
    {
        _preBuildCalled++;
    }

    public int GetPreBuildCalledCount() => _preBuildCalled;
}
