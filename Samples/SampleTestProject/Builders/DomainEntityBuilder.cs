using Buildenator.Abstraction;
using SampleProject;

namespace SampleTestProject.Builders;

[MakeBuilder(typeof(DomainEntity), generateStaticPropertyForBuilderCreation: true)]
public partial class DomainEntityBuilder
{
}