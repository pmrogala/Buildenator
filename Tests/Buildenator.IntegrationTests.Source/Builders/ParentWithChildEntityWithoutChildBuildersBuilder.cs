using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

/// <summary>
/// Builder for ParentWithChildEntity with UseChildBuilders disabled.
/// This should NOT generate additional With methods that accept Func&lt;ChildBuilder, ChildBuilder&gt;.
/// </summary>
[MakeBuilder(typeof(ParentWithChildEntity), generateDefaultBuildMethod: false, useChildBuilders: false)]
public partial class ParentWithChildEntityWithoutChildBuildersBuilder
{
}
