using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders.SubBuilders
{
	[MakeBuilder(typeof(Entity), generateDefaultBuildMethod: false)]
	public partial class EntityBuilder
	{
	}
}
