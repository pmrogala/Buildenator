using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

/// <summary>
/// Builder for EntityWithDictionary to test dictionary handling in code generation.
/// </summary>
[MakeBuilder(typeof(EntityWithDictionary))]
public partial class EntityWithDictionaryBuilder
{
}
