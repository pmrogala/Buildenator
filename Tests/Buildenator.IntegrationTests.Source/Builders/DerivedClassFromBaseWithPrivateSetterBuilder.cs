using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(DerivedClassFromBaseWithPrivateSetter), generateMethodsForUnreachableProperties: true)]
public partial class DerivedClassFromBaseWithPrivateSetterBuilder
{
}
