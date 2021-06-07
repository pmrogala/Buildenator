using FluentAssertions;
using SampleTestProject.Builders;
using Xunit;

namespace SampleTestProject
{
    public class DomainEntityTests
    {
        [Fact]
        public void DoMagic_SetStringLengthToIntAndIntToString()
        {
            var entity = DomainEntityBuilder
                .DomainEntity.WithPropertyIntGetter(1).WithPropertyStringGetter("a").Build();
            entity.DoMagic(2, "bar");

            entity.PropertyIntGetter.Should().Be(3);
            entity.PropertyStringGetter.Should().Be("2");
        }
    }
}
