using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(DefaultConstructor), generateDefaultBuildMethod: false)]
    public partial class DefaultConstructorBuilder
    {
        public DefaultConstructorBuilder()
        {

        }
    }
}
