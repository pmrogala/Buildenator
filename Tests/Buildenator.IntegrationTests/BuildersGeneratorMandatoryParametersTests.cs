using Buildenator.IntegrationTests.SharedEntities;
using Buildenator.IntegrationTests.SourceWithoutAssemblyInfo;
using FluentAssertions;
using Xunit;

namespace Buildenator.IntegrationTests;

public class BuildersGeneratorMandatoryParametersTests
{
    [Fact]
    public void BuildersGenerator_MandatoryParameters_BuilderRequiresConstructorArguments()
    {
        var result = new EntityWithMandatoryConstructorParametersBuilder(42, "shipping", ["tag1", "tag2"])
            .Build();

        result.LineNumber.Should().Be(42);
        result.Activity.Should().Be("shipping");
        result.Tags.Should().BeEquivalentTo(["tag1", "tag2"]);
    }

    [Fact]
    public void BuildersGenerator_MandatoryParameters_WithMethodsOverrideConstructorValues()
    {
        var result = new EntityWithMandatoryConstructorParametersBuilder(1, "initial", null)
            .WithLineNumber(99)
            .WithActivity("updated")
            .Build();

        result.LineNumber.Should().Be(99);
        result.Activity.Should().Be("updated");
    }

    [Fact]
    public void BuildersGenerator_MandatoryParameters_CollectionPropertiesStillWork()
    {
        var result = new EntityWithMandatoryConstructorParametersBuilder(1, "test", null)
            .AddToTags("tag1", "tag2")
            .Build();

        result.Tags.Should().BeEquivalentTo(new[] { "tag1", "tag2" });
    }

    [Fact]
    public void BuildersGenerator_MandatoryParameters_NoStaticFactoryPropertyGenerated()
    {
        var builderType = typeof(EntityWithMandatoryConstructorParametersBuilder);

        var staticProperty = builderType.GetProperty(
            nameof(EntityWithMandatoryConstructorParameters),
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        staticProperty.Should().BeNull("static factory property is incompatible with mandatory constructor parameters");
    }
}
