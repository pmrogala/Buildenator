using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using Buildenator.Generators;
using FluentAssertions;
using Moq;
using Xunit;

namespace Buildenator.UnitTests.Generators
{
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
			typedSymbolMock.Setup(ts => ts.NeedsFieldInit()).Returns(true);
			typedSymbolMock.Setup(ts => ts.GenerateFieldInitialization()).Returns("TestFieldInitialization");
			entityMock.Setup(e => e.GetAllUniqueSettablePropertiesAndParameters())
				.Returns(new[] { typedSymbolMock.Object });
			fixtureConfigurationMock.Setup(fc => fc.NeedsAdditionalConfiguration()).Returns(true);
			fixtureConfigurationMock.Setup(fc => fc.GenerateAdditionalConfiguration())
				.Returns("TestAdditionalConfiguration");

			// Act
			var result =
				ConstructorsGenerator.GenerateConstructor(builderName, entityMock.Object,
					fixtureConfigurationMock.Object);

			// Assert
			result.Should().NotBeNullOrEmpty();
			result.Should().Contain(builderName);
			result.Should().Contain("public TestBuilder()");
			result.Should().Contain("TestFieldInitialization");
			result.Should().Contain("TestAdditionalConfiguration");
		}

		[Fact]
		public void GenerateConstructor_WithNullFixtureConfiguration_ShouldGenerateValidOutput()
		{
			// Arrange
			var builderName = "TestBuilder";
			var entityMock = new Mock<IEntityToBuild>();
			IFixtureProperties? fixtureConfiguration = null;
			var typedSymbolMock = new Mock<ITypedSymbol>();
			typedSymbolMock.Setup(ts => ts.NeedsFieldInit()).Returns(true);
			typedSymbolMock.Setup(ts => ts.GenerateFieldInitialization()).Returns("TestFieldInitialization");
			entityMock.Setup(e => e.GetAllUniqueSettablePropertiesAndParameters())
				.Returns(new[] { typedSymbolMock.Object });

			// Act
			var result =
				ConstructorsGenerator.GenerateConstructor(builderName, entityMock.Object, fixtureConfiguration);

			// Assert
			result.Should().NotBeNullOrEmpty();
			result.Should().Contain(builderName);
			result.Should().Contain("public TestBuilder()");
			result.Should().Contain("TestFieldInitialization");
			result.Should().NotContain("TestAdditionalConfiguration");
		}

		[Fact]
		public void GenerateConstructor_WithNoNeedsFieldInit_ShouldGenerateValidOutput()
		{
			// Arrange
			var builderName = "TestBuilder";
			var entityMock = new Mock<IEntityToBuild>();
			var fixtureConfigurationMock = new Mock<IFixtureProperties>();
			var typedSymbolMock = new Mock<ITypedSymbol>();
			typedSymbolMock.Setup(ts => ts.NeedsFieldInit()).Returns(false);
			entityMock.Setup(e => e.GetAllUniqueSettablePropertiesAndParameters())
				.Returns(new[] { typedSymbolMock.Object });
			fixtureConfigurationMock.Setup(fc => fc.NeedsAdditionalConfiguration()).Returns(false);

			// Act
			var result =
				ConstructorsGenerator.GenerateConstructor(builderName, entityMock.Object,
					fixtureConfigurationMock.Object);

			// Assert
			result.Should().BeEmpty();
		}
	}
}