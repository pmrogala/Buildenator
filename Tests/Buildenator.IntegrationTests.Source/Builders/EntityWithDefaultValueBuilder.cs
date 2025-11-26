using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(EntityWithDefaultValue), generateDefaultBuildMethod: false, generateStaticPropertyForBuilderCreation: true)]
public partial class EntityWithDefaultValueBuilder
{
    // User-defined default values using the Default{PropertyName} naming convention
    public const string DefaultName = "DefaultEntityName";
    public const int DefaultCount = 42;
    public static readonly string DefaultOptionalValue = "DefaultOptional";
}
