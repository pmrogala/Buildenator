using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(EntityWithNullableDictionary))]
    public partial class EntityWithNullableDictionaryBuilder
    {
    }
}
