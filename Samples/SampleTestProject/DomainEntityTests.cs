using FluentAssertions;
using SampleTestProject.Builders;
using Xunit;

namespace SampleTestProject;

public class DomainEntityTests
{
    [Fact]
    public void DoMagic_SetStringLengthToIntAndIntToString()
    {
        var entity = DomainEntityBuilder
            .DomainEntity.WithPropertyIntGetter(1).WithPropertyStringGetter("a").Build();
        entity.DoMagic(2, "bar");

        _ = entity.PropertyIntGetter.Should().Be(3);
        _ = entity.PropertyStringGetter.Should().Be("2");
    }
}