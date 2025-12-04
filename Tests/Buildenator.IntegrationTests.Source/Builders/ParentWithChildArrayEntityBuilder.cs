using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

/// <summary>
/// Builder for ParentWithChildArrayEntity with UseChildBuilders enabled.
/// This will generate AddTo methods that accept Func&lt;ChildBuilder, ChildBuilder&gt; for arrays containing buildable entities.
/// </summary>
[MakeBuilder(typeof(ParentWithChildArrayEntity), generateDefaultBuildMethod: false, useChildBuilders: true)]
public partial class ParentWithChildArrayEntityBuilder
{
}
