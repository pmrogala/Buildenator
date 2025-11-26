using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

/// <summary>
/// Builder with initializeCollectionsWithEmpty = true.
/// All collection fields should be initialized with empty collections in the constructor.
/// </summary>
[MakeBuilder(typeof(EntityWithCollectionsForEmptyInit), initializeCollectionsWithEmpty: true)]
public partial class EntityWithCollectionsForEmptyInitBuilder
{
}
