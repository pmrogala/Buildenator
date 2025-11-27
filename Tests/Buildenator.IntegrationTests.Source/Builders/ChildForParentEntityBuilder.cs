using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

/// <summary>
/// Builder for ChildForParentEntity, used to test UseChildBuilders feature.
/// </summary>
[MakeBuilder(typeof(ChildForParentEntity), generateDefaultBuildMethod: false)]
public partial class ChildForParentEntityBuilder
{
}
