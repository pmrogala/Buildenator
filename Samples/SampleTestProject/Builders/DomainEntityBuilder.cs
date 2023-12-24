using Buildenator.Abstraction;
using SampleProject;

namespace SampleTestProject.Builders;

[MakeBuilder(typeof(DomainEntity))]
public partial class DomainEntityBuilder
{
}