using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

/// <summary>
/// Builder for ParentWithChildCollectionEntity with UseChildBuilders enabled.
/// This will generate AddTo methods that accept Func&lt;ChildBuilder, ChildBuilder&gt; for collections containing buildable entities.
/// </summary>
[MakeBuilder(typeof(ParentWithChildCollectionEntity), generateDefaultBuildMethod: false, useChildBuilders: true)]
public partial class ParentWithChildCollectionEntityBuilder
{
}
