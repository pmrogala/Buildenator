using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(DefaultConstructor), generateDefaultBuildMethod: false)]
public partial class DefaultConstructorBuilder
{
    public DefaultConstructorBuilder()
    {

        }
}