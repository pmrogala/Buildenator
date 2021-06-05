using AutoFixture.Xunit2;
using Buildenator.UnitTests.Source.Builders;
using FluentAssertions;
using Xunit;

namespace Buildenator.UnitTests
{
    public class BuilderGeneratorTests
    {
        [Fact]
        public void BuilderGenerator_HasStaticBulderFactory()
        {
            var builder = DomainEntityBuilder.DomainEntity;

            builder.Should().NotBeNull();
        }

        [Theory]
        [AutoData]
        public void BuilderGenerator_CreatedWithMethodsByContructorParametersNames(int value, string str)
        {
            var builder = DomainEntityBuilder.DomainEntity;

            var entity = builder.WithPropertyIntGetter(value).WithPropertyStringGetter(str).Build();
            entity.PropertyIntGetter.Should().Be(value);
            entity.PropertyGetter.Should().Be(str);
        }
    }
}
