using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using Buildenator.Generators;
using FluentAssertions;
using Moq;
using Xunit;

namespace Buildenator.UnitTests.Generators;

public class ConstructorsGeneratorTests
{
    [Fact]
    public void GenerateConstructor_WithValidInput_ShouldGenerateValidOutput()
    {
        // Arrange
        var builderName = "TestBuilder";
        var entityMock = new Mock<IEntityToBuild>();
        var fixtureConfigurationMock = new Mock<IFixtureProperties>();
        var typedSymbolMock = new Mock<ITypedSymbol>();
        _ = typedSymbolMock.Setup(ts => ts.NeedsFieldInit()).Returns(true);
        _ = typedSymbolMock.Setup(ts => ts.GenerateFieldInitialization()).Returns("TestFieldInitialization");
        _ = entityMock.Setup(e => e.GetAllUniqueSettablePropertiesAndParameters())
            .Returns(new[] { typedSymbolMock.Object });
        _ = fixtureConfigurationMock.Setup(fc => fc.NeedsAdditionalConfiguration()).Returns(true);
        _ = fixtureConfigurationMock.Setup(fc => fc.GenerateAdditionalConfiguration())
            .Returns("TestAdditionalConfiguration");

        // Act
        var result =
            ConstructorsGenerator.GenerateConstructor(builderName, entityMock.Object,
                fixtureConfigurationMock.Object);

        // Assert
        _ = result.Should().NotBeNullOrEmpty();
        _ = result.Should().Contain(builderName);
        _ = result.Should().Contain("public TestBuilder()");
        _ = result.Should().Contain("TestFieldInitialization");
        _ = result.Should().Contain("TestAdditionalConfiguration");
    }

    [Fact]
    public void GenerateConstructor_WithNullFixtureConfiguration_ShouldGenerateValidOutput()
    {
        // Arrange
        var builderName = "TestBuilder";
        var entityMock = new Mock<IEntityToBuild>();
        IFixtureProperties? fixtureConfiguration = null;
        var typedSymbolMock = new Mock<ITypedSymbol>();
        _ = typedSymbolMock.Setup(ts => ts.NeedsFieldInit()).Returns(true);
        _ = typedSymbolMock.Setup(ts => ts.GenerateFieldInitialization()).Returns("TestFieldInitialization");
        _ = entityMock.Setup(e => e.GetAllUniqueSettablePropertiesAndParameters())
            .Returns(new[] { typedSymbolMock.Object });

        // Act
        var result =
            ConstructorsGenerator.GenerateConstructor(builderName, entityMock.Object, fixtureConfiguration);

        // Assert
        _ = result.Should().NotBeNullOrEmpty();
        _ = result.Should().Contain(builderName);
        _ = result.Should().Contain("public TestBuilder()");
        _ = result.Should().Contain("TestFieldInitialization");
        _ = result.Should().NotContain("TestAdditionalConfiguration");
    }

    [Fact]
    public void GenerateConstructor_WithNoNeedsFieldInit_ShouldGenerateValidOutput()
    {
        // Arrange
        var builderName = "TestBuilder";
        var entityMock = new Mock<IEntityToBuild>();
        var fixtureConfigurationMock = new Mock<IFixtureProperties>();
        var typedSymbolMock = new Mock<ITypedSymbol>();
        _ = typedSymbolMock.Setup(ts => ts.NeedsFieldInit()).Returns(false);
        _ = entityMock.Setup(e => e.GetAllUniqueSettablePropertiesAndParameters())
            .Returns(new[] { typedSymbolMock.Object });
        _ = fixtureConfigurationMock.Setup(fc => fc.NeedsAdditionalConfiguration()).Returns(false);

        // Act
        var result =
            ConstructorsGenerator.GenerateConstructor(builderName, entityMock.Object,
                fixtureConfigurationMock.Object);

        // Assert
        _ = result.Should().BeEmpty();
    }
}