using System;
using Buildenator.Abstraction;
using Buildenator.Configuration;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Buildenator.UnitTests.Configuration;

public class BuilderPropertiesTests
{
    private readonly Mock<INamedTypeSymbol> _builderSymbolMock;

    public BuilderPropertiesTests()
    {
        _builderSymbolMock = new Mock<INamedTypeSymbol>();
        var namespaceSymbol = new Mock<INamespaceSymbol>();
        _ = namespaceSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("Test");
        _ = _builderSymbolMock.Setup(
                x => x.ToDisplayString(
                    It.Is<SymbolDisplayFormat>(a => a.GenericsOptions == SymbolDisplayGenericsOptions.IncludeTypeParameters)))
            .Returns("BuilderFullName");
        _ = _builderSymbolMock.Setup(x => x.ContainingNamespace).Returns(namespaceSymbol.Object);
        _ = _builderSymbolMock.Setup(x => x.Name).Returns("BuilderName");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBuildingMethodsPrefixIsEmpty()
    {
        // Arrange
        var attributeDataMock = new MakeBuilderAttributeInternal(null!, string.Empty, false,
            NullableStrategy.Enabled, false, true);

        // Act
        var act = () => Create(attributeDataMock);

        // Assert
        _ = act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldInitializePropertiesAndCollections_WhenValidParametersAreProvided()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false,
            NullableStrategy.Enabled, false, true);
        var methodSymbolMock = new Mock<IMethodSymbol>();
        _ = methodSymbolMock.SetupGet(x => x.Name).Returns("BuildMethod");
        _ = methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Ordinary);
        _ = _builderSymbolMock.Setup(x => x.GetMembers()).Returns([methodSymbolMock.Object]);

        // Act
        var properties = Create(attributeDataMock);

        // Assert
        _ = properties.ContainingNamespace.Should().NotBeNullOrEmpty();
        _ = properties.Name.Should().NotBeNullOrEmpty();
        _ = properties.FullName.Should().NotBeNullOrEmpty();
        _ = properties.BuildingMethodsPrefix.Should().Be(attributeDataMock.BuildingMethodsPrefix);
        _ = properties.NullableStrategy.Should().Be(attributeDataMock.NullableStrategy!.Value);
        _ = properties.StaticCreator.Should().Be(attributeDataMock.DefaultStaticCreator!.Value);
        _ = properties.ImplicitCast.Should().Be(attributeDataMock.ImplicitCast!.Value);
        _ = properties.ShouldGenerateMethodsForUnreachableProperties.Should().Be(attributeDataMock.GenerateMethodsForUnreachableProperties!.Value);
        _ = properties.IsPostBuildMethodOverriden.Should().BeFalse();
        _ = properties.IsDefaultConstructorOverriden.Should().BeFalse();
        _ = properties.BuildingMethods.Should().ContainKey(methodSymbolMock.Object.Name).And.ContainValue(methodSymbolMock.Object);
        _ = properties.Fields.Should().BeEmpty();
    }

    private BuilderProperties Create(MakeBuilderAttributeInternal attributeDataMock)
    {
        return BuilderProperties.Create(_builderSymbolMock.Object, attributeDataMock, null);
    }

    [Theory]
    [InlineData("Build", 1, false, Accessibility.Public)]
    [InlineData("Build", 0, false, Accessibility.Public)]
    [InlineData("With", 1, false, Accessibility.Public)]
    [InlineData("Build", 0, true, Accessibility.Public)]
    [InlineData("Build", 1, false, Accessibility.Private)]
    [InlineData("Build", 0, false, Accessibility.Private)]
    [InlineData("With", 1, false, Accessibility.Private)]
    [InlineData("Build", 0, true, Accessibility.Private)]
    [InlineData("Build", 1, false, Accessibility.Protected)]
    [InlineData("Build", 0, false, Accessibility.Protected)]
    [InlineData("With", 1, false, Accessibility.Protected)]
    [InlineData("Build", 0, true, Accessibility.Protected)]
    [InlineData("Build", 1, false, Accessibility.Internal)]
    [InlineData("Build", 0, false, Accessibility.Internal)]
    [InlineData("With", 1, false, Accessibility.Internal)]
    [InlineData("Build", 0, true, Accessibility.Internal)]
    public void Constructor_ShouldOnlyPopulateBuildingMethodsDictionary_WhenOrdinaryBuildingMethodIsFound(
        string prefix, int parametersLength, bool isImplicitlyDeclared, Accessibility accessibility)
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, prefix, false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        _ = methodSymbolMock.SetupGet(x => x.Name).Returns($"{prefix}Method");
        _ = methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Ordinary);
        _ = methodSymbolMock.SetupGet(x => x.DeclaredAccessibility).Returns(accessibility);
        if (parametersLength == 0)
            _ = methodSymbolMock.SetupGet(x => x.Parameters).Returns([]);
        else
            _ = methodSymbolMock.SetupGet(x => x.Parameters)
                .Returns([new Mock<IParameterSymbol>().Object]);
        _ = methodSymbolMock.SetupGet(x => x.IsImplicitlyDeclared).Returns(isImplicitlyDeclared);

        _ = _builderSymbolMock.Setup(x => x.GetMembers()).Returns([methodSymbolMock.Object]);

        // Act
        var properties = BuilderProperties.Create(_builderSymbolMock.Object, attributeDataMock, null);

        // Assert
        _ = properties.BuildingMethods.Should().ContainKey(methodSymbolMock.Object.Name).And.ContainValue(methodSymbolMock.Object);
        _ = properties.Fields.Should().BeEmpty();
        _ = properties.IsDefaultConstructorOverriden.Should().BeFalse();
        _ = properties.IsPostBuildMethodOverriden.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldOnlySetIsPostBuildMethodOverrideToTrue_WhenPostBuildMethodIsFound()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        _ = methodSymbolMock.SetupGet(x => x.Name).Returns(DefaultConstants.PostBuildMethodName);
        _ = methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Ordinary);

        _ = _builderSymbolMock.Setup(x => x.GetMembers()).Returns([methodSymbolMock.Object]);

        // Act
        var properties = Create(attributeDataMock);

        // Assert
        _ = properties.IsPostBuildMethodOverriden.Should().BeTrue();
        _ = properties.BuildingMethods.Should().BeEmpty();
        _ = properties.Fields.Should().BeEmpty();
        _ = properties.IsDefaultConstructorOverriden.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldOnlySetIsDefaultConstructorOverrideToTrue_WhenTheDefaultConstructorHasTheSameNameAsThePrefix()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        _ = methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Constructor);
        _ = methodSymbolMock.SetupGet(x => x.Parameters).Returns([]);
        _ = methodSymbolMock.SetupGet(x => x.Name).Returns(attributeDataMock.BuildingMethodsPrefix!);
        _ = methodSymbolMock.SetupGet(x => x.IsImplicitlyDeclared).Returns(false);

        _ = _builderSymbolMock.Setup(x => x.GetMembers()).Returns([methodSymbolMock.Object]);

        // Act
        var properties = Create(attributeDataMock);

        // Assert
        _ = properties.IsDefaultConstructorOverriden.Should().BeTrue();
        _ = properties.IsPostBuildMethodOverriden.Should().BeFalse();
        _ = properties.BuildingMethods.Should().BeEmpty();
        _ = properties.Fields.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldOnlySetIsDefaultConstructorOverrideToTrue_WhenDefaultConstructorIsFound()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        _ = methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Constructor);
        _ = methodSymbolMock.SetupGet(x => x.Parameters).Returns([]);
        _ = methodSymbolMock.SetupGet(x => x.Name).Returns("Constructor");
        _ = methodSymbolMock.SetupGet(x => x.IsImplicitlyDeclared).Returns(false);

        _ = _builderSymbolMock.Setup(x => x.GetMembers()).Returns([methodSymbolMock.Object]);

        // Act
        var properties = Create(attributeDataMock);

        // Assert
        _ = properties.IsDefaultConstructorOverriden.Should().BeTrue();
        _ = properties.IsPostBuildMethodOverriden.Should().BeFalse();
        _ = properties.BuildingMethods.Should().BeEmpty();
        _ = properties.Fields.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldOnlyAddFieldToFieldsDictionary_WhenFieldIsFound()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var fieldSymbolMock = new Mock<IFieldSymbol>();
        _ = fieldSymbolMock.SetupGet(x => x.Name).Returns("SomeField");

        _ = _builderSymbolMock.Setup(x => x.GetMembers()).Returns([fieldSymbolMock.Object]);

        // Act
        var properties = Create(attributeDataMock);

        // Assert
        _ = properties.Fields.Should().ContainKey(fieldSymbolMock.Object.Name).And.ContainValue(fieldSymbolMock.Object);
        _ = properties.IsDefaultConstructorOverriden.Should().BeFalse();
        _ = properties.IsPostBuildMethodOverriden.Should().BeFalse();
        _ = properties.BuildingMethods.Should().BeEmpty();
    }
}